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
    # envelope de onset (mais sensivel: lag menor, sem media movel longa)
    onset_strength = librosa.onset.onset_strength(y=y, sr=sr)
    # onsets (ataques) — ALTA sensibilidade para nao perder notas no solo do acordeao.
    # O excesso e' limpo depois pela quantizacao na grade (1 nota por casa).
    onset_frames = librosa.onset.onset_detect(
        y=y, sr=sr, onset_envelope=onset_strength, backtrack=True, units="frames",
        pre_max=3, post_max=3, pre_avg=10, post_avg=10, delta=0.03, wait=3,
    )
    onset_times = librosa.frames_to_time(onset_frames, sr=sr)

    # GATE DE ENERGIA: descarta onsets onde o acordeao esta em silencio
    # (evita "nota passando no silencio"). Usa RMS normalizado.
    rms = librosa.feature.rms(y=y)[0]
    rms_times = librosa.times_like(rms, sr=sr)
    rms_norm = rms / (rms.max() + 1e-9)
    gate = 0.06  # ~6% do pico = limiar de "tem som"
    kept_frames, kept_times = [], []
    for f, t in zip(onset_frames, onset_times):
        i = int(np.argmin(np.abs(rms_times - t)))
        if rms_norm[i] >= gate:
            kept_frames.append(f)
            kept_times.append(t)
    n_drop = len(onset_times) - len(kept_times)
    onset_frames = np.array(kept_frames, dtype=int)
    onset_times = np.array(kept_times)
    print(f"gate de energia: {n_drop} onset(s) em silencio descartado(s)")

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
    n = len(times)
    pitches = np.array([pitch_at(t, f0, f0_times) for t in times])
    valid = pitches[~np.isnan(pitches)]
    if n < 4 or len(valid) < 4:
        # sem pitch confiavel -> alterna lanes
        return [i % 4 for i in range(n)]
    # forward-fill dos NaN (nota sustentada herda a altura anterior)
    filled = pitches.copy()
    last = valid[0]
    for i in range(n):
        if np.isnan(filled[i]):
            filled[i] = last
        else:
            last = filled[i]
    # rank ORDINAL (empates desfeitos por ordem temporal) -> 4 faixas balanceadas.
    # Garante uso das 4 lanes (sem coluna morta) e mantem 'mais agudo -> lane maior'.
    ranks = np.argsort(np.argsort(filled, kind="stable"), kind="stable")
    lanes = [int(r * 4 // n) for r in ranks]
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


def quantize(times, bpm, subdiv):
    """Trava cada onset na casa mais proxima da grade musical e deduplica:
    1 nota por casa (subdiv=2 -> colcheias, subdiv=1 -> seminimas).
    Mantem a nota onde HA ataque (so gera onde o acordeao toca), mas com
    o tempo alinhado a' batida -> sensacao 'fiel e no tempo'.
    """
    slot = (60.0 / bpm) / subdiv
    seen = set()
    out = []
    for t in sorted(times):
        k = int(round(t / slot))
        if k not in seen:
            seen.add(k)
            out.append(k * slot)
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
    # hard_gap (opcional, padrao 0.42): espacamento minimo do Hard em fracao de batida.
    # MAIOR = menos notas (Hard mais ralo). So afeta o Hard.
    hard_gap = float(sys.argv[4]) if len(sys.argv) > 4 else 0.42
    onset_times, strengths, beat_times, f0, f0_times = detect(path, bpm)
    print(f"onsets detectados: {len(onset_times)} | batidas: {len(beat_times)}")

    # HARD: TEMPOS REAIS dos onsets (fiel ao ataque do acordeao, sem quantizar na grade,
    # que tiraria do tempo por causa do swing/rubato). thin so' remove disparos-duplos.
    beat = 60.0 / bpm
    hard_times = thin(sorted(onset_times), min_gap=beat * hard_gap)
    hard_lanes = assign_lanes(hard_times, f0, f0_times)
    build_track(hard_times, hard_lanes, bpm).save(out + "-Hard.mid")
    print(f"HARD: {len(hard_times)} notas -> {out}-Hard.mid")

    # EASY: onsets reais perto de uma batida forte (1 por batida) — tempo do proprio ataque.
    tol = beat * 0.35
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
