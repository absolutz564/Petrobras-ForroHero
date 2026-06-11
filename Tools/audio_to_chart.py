"""
Gera arquivos .mid de chart (rhythm game 4 lanes) a partir de um stem de melodia.
- Detecta onsets (ataques) na melodia (Accordion).
- Estima o pitch em cada onset e mapeia o contorno em 4 lanes (grave->lane0, aguda->lane3).
- Easy: notas nas batidas fortes (densidade menor). Hard: mais onsets (densidade maior).
- Saida: 4 alturas MIDI fixas (60,62,64,65) = lanes 0,1,2,3.
Uso: python audio_to_chart.py <accordion.wav> <bpm> <saida_sem_extensao>
"""
import sys
import numpy as np
import librosa
import mido

LANE_PITCHES = [60, 62, 64, 65]  # lane 0,1,2,3
TICKS_PER_BEAT = 480


def seconds_to_ticks(t, bpm):
    return int(round(t * TICKS_PER_BEAT * bpm / 60.0))


def detect(path, bpm):
    y, sr = librosa.load(path, mono=True)
    # onsets (ataques)
    onset_frames = librosa.onset.onset_detect(y=y, sr=sr, backtrack=True, units="frames")
    onset_times = librosa.frames_to_time(onset_frames, sr=sr)
    onset_strength = librosa.onset.onset_strength(y=y, sr=sr)
    # forca de cada onset
    strengths = []
    for f in onset_frames:
        idx = min(f, len(onset_strength) - 1)
        strengths.append(float(onset_strength[idx]))
    strengths = np.array(strengths) if len(strengths) else np.array([])
    # batidas reais (para o Easy alinhar nas fortes)
    _, beat_frames = librosa.beat.beat_track(y=y, sr=sr, bpm=bpm)
    beat_times = librosa.frames_to_time(beat_frames, sr=sr)
    # pitch (f0) para o contorno -> lanes
    f0, voiced, _ = librosa.pyin(
        y, fmin=librosa.note_to_hz("C2"), fmax=librosa.note_to_hz("C6"), sr=sr
    )
    f0_times = librosa.times_like(f0, sr=sr)
    return onset_times, strengths, beat_times, f0, f0_times


def pitch_at(t, f0, f0_times):
    i = int(np.argmin(np.abs(f0_times - t)))
    # procura f0 valido proximo
    for off in range(0, 8):
        for j in (i - off, i + off):
            if 0 <= j < len(f0) and not np.isnan(f0[j]):
                return f0[j]
    return np.nan


def assign_lanes(times, f0, f0_times):
    pitches = np.array([pitch_at(t, f0, f0_times) for t in times])
    valid = pitches[~np.isnan(pitches)]
    if len(valid) < 4:
        # sem pitch confiavel -> alterna lanes
        return [i % 4 for i in range(len(times))]
    # 4 faixas por quantis sobre as alturas da musica
    edges = np.quantile(valid, [0.25, 0.5, 0.75])
    lanes = []
    last = 0
    for p in pitches:
        if np.isnan(p):
            lanes.append(last)  # mantem lane anterior
        else:
            lane = int(np.searchsorted(edges, p))  # 0..3
            lanes.append(lane)
            last = lane
    return lanes


def thin(times, min_gap):
    """remove notas muito proximas (mantem a primeira)."""
    out = []
    last = -1e9
    for t in times:
        if t - last >= min_gap:
            out.append(t)
            last = t
    return out


def build_track(times, lanes, bpm):
    mid = mido.MidiFile(ticks_per_beat=TICKS_PER_BEAT)
    track = mido.MidiTrack()
    mid.tracks.append(track)
    track.append(mido.MetaMessage("track_name", name="Chart", time=0))
    track.append(mido.MetaMessage("set_tempo", tempo=mido.bpm2tempo(bpm), time=0))
    note_len = seconds_to_ticks(0.12, bpm)  # tap curto
    events = []
    for t, lane in zip(times, lanes):
        on = seconds_to_ticks(t, bpm)
        events.append((on, "on", LANE_PITCHES[lane]))
        events.append((on + note_len, "off", LANE_PITCHES[lane]))
    events.sort(key=lambda e: (e[0], e[1] == "on"))
    prev = 0
    for tick, kind, pitch in events:
        delta = max(0, tick - prev)
        prev = tick
        if kind == "on":
            track.append(mido.Message("note_on", note=pitch, velocity=100, time=delta))
        else:
            track.append(mido.Message("note_off", note=pitch, velocity=0, time=delta))
    return mid


def main():
    path, bpm, out = sys.argv[1], float(sys.argv[2]), sys.argv[3]
    onset_times, strengths, beat_times, f0, f0_times = detect(path, bpm)
    print(f"onsets detectados: {len(onset_times)} | batidas: {len(beat_times)}")

    # HARD: todos os onsets, espacamento minimo de ~1/4 de batida
    beat = 60.0 / bpm
    hard_times = thin(sorted(onset_times), min_gap=beat * 0.45)
    hard_lanes = assign_lanes(hard_times, f0, f0_times)
    build_track(hard_times, hard_lanes, bpm).save(out + "-Hard.mid")
    print(f"HARD: {len(hard_times)} notas -> {out}-Hard.mid")

    # EASY: onsets que caem perto de uma batida forte (tolerancia), 1 por batida
    tol = beat * 0.25
    easy_times = []
    for bt in beat_times:
        near = [t for t in onset_times if abs(t - bt) <= tol]
        if near:
            easy_times.append(min(near, key=lambda t: abs(t - bt)))
    easy_times = thin(sorted(easy_times), min_gap=beat * 0.9)
    easy_lanes = assign_lanes(easy_times, f0, f0_times)
    build_track(easy_times, easy_lanes, bpm).save(out + "-Easy.mid")
    print(f"EASY: {len(easy_times)} notas -> {out}-Easy.mid")


if __name__ == "__main__":
    main()
