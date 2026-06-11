"""
Gera um RhythmTimelineAsset (.asset) COMPLETO a partir de um .mid,
replicando a estrutura de um chart que funciona (Brincar-de-Amar).
- 4 Rhythm Tracks (IDs 0-3) com as notas (RhythmClips + NoteDefinition por lane)
- 2 Audio Tracks: Back (1a) e Accordion (2a)
Sobrescreve o .asset alvo SEM tocar no .meta (preserva o GUID).

Uso:
  python gen_chart.py <midi> <bpm> <back_wav> <accordion_wav> <out_asset>
"""
import sys
import os
import soundfile as sf
import mido

# NoteDefinitions (Note 0..3 Tap) por lane
NOTE_DEF_GUID = {
    0: "e40233176e3ec0949b5d487f60496c7b",
    1: "cb46a16f54c343a459fc498ebbe18c47",
    2: "eb55ee6bb96db884594edbc5e44c7d54",
    3: "3412d6be277363b4480a249f9e36684d",
}
PITCH_TO_LANE = {60: 0, 62: 1, 64: 2, 65: 3}

SCRIPT_TIMELINE = "9a0313bfa62342a0a831dde59d6a1764"
SCRIPT_RHYTHMTRACK = "be2b88710e4502a45800deefbabf1b37"
SCRIPT_RHYTHMCLIP = "dfa41e5e51d007546bb3d3ad7cdb93ca"
SCRIPT_AUDIOTRACK = "8b22792c3b570444eb18cb78c2af3a74"
SCRIPT_AUDIOPLAYABLE = "4f10dd60657c6004587f237a7e90f8e4"

CURVE = """    m_MixInCurve:
      serializedVersion: 2
      m_Curve:
      - serializedVersion: 3
        time: 0
        value: 0
        inSlope: 0
        outSlope: 0
        tangentMode: 0
        weightedMode: 0
        inWeight: 0
        outWeight: 0
      - serializedVersion: 3
        time: 1
        value: 1
        inSlope: 0
        outSlope: 0
        tangentMode: 0
        weightedMode: 0
        inWeight: 0
        outWeight: 0
      m_PreInfinity: 2
      m_PostInfinity: 2
      m_RotationOrder: 4
    m_MixOutCurve:
      serializedVersion: 2
      m_Curve:
      - serializedVersion: 3
        time: 0
        value: 1
        inSlope: 0
        outSlope: 0
        tangentMode: 0
        weightedMode: 0
        inWeight: 0
        outWeight: 0
      - serializedVersion: 3
        time: 1
        value: 0
        inSlope: 0
        outSlope: 0
        tangentMode: 0
        weightedMode: 0
        inWeight: 0
        outWeight: 0
      m_PreInfinity: 2
      m_PostInfinity: 2
      m_RotationOrder: 4"""


def read_guid(wav_path):
    with open(wav_path + ".meta", "r", encoding="utf-8") as f:
        for line in f:
            if line.startswith("guid:"):
                return line.split(":", 1)[1].strip()
    raise RuntimeError("guid nao encontrado em " + wav_path + ".meta")


def mono_header(anchor, hide=1):
    return (f"--- !u!114 &{anchor}\n"
            "MonoBehaviour:\n"
            f"  m_ObjectHideFlags: {hide}\n"
            "  m_CorrespondingSourceObject: {fileID: 0}\n"
            "  m_PrefabInstance: {fileID: 0}\n"
            "  m_PrefabAsset: {fileID: 0}\n"
            "  m_GameObject: {fileID: 0}\n"
            "  m_Enabled: 1\n"
            "  m_EditorHideFlags: 0\n")


def timeline_clip(start, duration, asset_id, track_id, display, audio=False):
    blend = "-1" if audio else "0"
    return (
        "  - m_Version: 1\n"
        f"    m_Start: {start}\n"
        "    m_ClipIn: 0\n"
        f"    m_Asset: {{fileID: {asset_id}}}\n"
        f"    m_Duration: {duration}\n"
        "    m_TimeScale: 1\n"
        f"    m_ParentTrack: {{fileID: {track_id}}}\n"
        "    m_EaseInDuration: 0\n"
        "    m_EaseOutDuration: 0\n"
        f"    m_BlendInDuration: {blend}\n"
        f"    m_BlendOutDuration: {blend}\n"
        f"{CURVE}\n"
        "    m_BlendInCurveMode: 0\n"
        "    m_BlendOutCurveMode: 0\n"
        "    m_ExposedParameterNames: []\n"
        "    m_AnimationCurves: {fileID: 0}\n"
        "    m_Recordable: 0\n"
        "    m_PostExtrapolationMode: 0\n"
        "    m_PreExtrapolationMode: 0\n"
        "    m_PostExtrapolationTime: 0\n"
        "    m_PreExtrapolationTime: 0\n"
        f"    m_DisplayName: {display}\n"
    )


def main():
    midi, bpm, back_wav, acc_wav, out = sys.argv[1:6]
    bpm = float(bpm)
    name = os.path.splitext(os.path.basename(out))[0]
    tap_dur = 30.0 / bpm

    back_guid = read_guid(back_wav)
    acc_guid = read_guid(acc_wav)
    info = sf.info(back_wav)
    audio_dur = info.frames / info.samplerate

    # parse midi -> (start_seconds, lane)
    mid = mido.MidiFile(midi)
    tempo = 500000
    for m in mid.tracks[0]:
        if m.type == "set_tempo":
            tempo = m.tempo
    notes = []
    t = 0
    for m in mid.tracks[0]:
        t += mido.tick2second(m.time, mid.ticks_per_beat, tempo)
        if m.type == "note_on" and m.velocity > 0:
            lane = PITCH_TO_LANE.get(m.note)
            if lane is not None:
                notes.append((t, lane))

    # fileID allocator
    counter = [200000000]

    def nid():
        counter[0] += 2
        return counter[0]

    rhythm_track_ids = {lane: nid() for lane in range(4)}
    back_track_id = nid()
    acc_track_id = nid()
    back_apa_id = nid()
    acc_apa_id = nid()

    # build rhythm clips + per-track clip lists
    clips_by_lane = {lane: [] for lane in range(4)}
    rhythmclip_blocks = []
    for start, lane in notes:
        clip_id = nid()
        clips_by_lane[lane].append(timeline_clip(
            start, tap_dur, clip_id, rhythm_track_ids[lane], '"\\u2001"'))
        rhythmclip_blocks.append(
            mono_header(clip_id)
            + f"  m_Script: {{fileID: 11500000, guid: {SCRIPT_RHYTHMCLIP}, type: 3}}\n"
            + "  m_Name: RhythmClip\n"
            + "  m_EditorClassIdentifier:\n"
            + "  m_RhythmPlayableBehaviour:\n"
            + f"    m_NoteDefinition: {{fileID: 11400000, guid: {NOTE_DEF_GUID[lane]}, type: 2}}\n"
            + "  m_ClipParameters:\n"
            + "    m_IntParameter: 0\n"
            + "    m_StringParameter:\n"
            + "    m_FloatParameter: 0\n"
            + "    m_Vector2Parameter: {x: 0, y: 0}\n"
            + "    m_Vector3Parameter: {x: 0, y: 0, z: 0}\n"
            + "    m_Vector4Parameter: {x: 0, y: 0, z: 0, w: 0}\n"
            + "    m_ColorParameter: {r: 0, g: 0, b: 0, a: 0}\n"
            + "    m_ObjectReferenceParameter: {fileID: 0}\n"
            + "    m_RhythmClipExtraNoteData: {fileID: 0}\n"
        )

    def rhythm_track(lane):
        tid = rhythm_track_ids[lane]
        body = (mono_header(tid)
                + f"  m_Script: {{fileID: 11500000, guid: {SCRIPT_RHYTHMTRACK}, type: 3}}\n"
                + f"  m_Name: Rhythm Track ({lane})\n"
                + "  m_EditorClassIdentifier:\n"
                + "  m_Version: 3\n"
                + "  m_AnimClip: {fileID: 0}\n"
                + "  m_Locked: 0\n"
                + "  m_Muted: 0\n"
                + "  m_CustomPlayableFullTypename:\n"
                + "  m_Curves: {fileID: 0}\n"
                + "  m_Parent: {fileID: 11400000}\n"
                + "  m_Children: []\n"
                + "  m_Clips:\n")
        clips = clips_by_lane[lane]
        body += "".join(clips) if clips else ""
        if not clips:
            body = body[:-len("  m_Clips:\n")] + "  m_Clips: []\n"
        body += ("  m_Markers:\n    m_Objects: []\n"
                 + f"  m_ID: {lane}\n")
        return body

    def audio_track(tid, apa_id, name_disp, dispname):
        body = (mono_header(tid)
                + f"  m_Script: {{fileID: 11500000, guid: {SCRIPT_AUDIOTRACK}, type: 3}}\n"
                + f"  m_Name: {name_disp}\n"
                + "  m_EditorClassIdentifier: Unity.Timeline::UnityEngine.Timeline.AudioTrack\n"
                + "  m_Version: 3\n"
                + "  m_AnimClip: {fileID: 0}\n"
                + "  m_Locked: 0\n"
                + "  m_Muted: 0\n"
                + "  m_CustomPlayableFullTypename:\n"
                + "  m_Curves: {fileID: 0}\n"
                + "  m_Parent: {fileID: 11400000}\n"
                + "  m_Children: []\n"
                + "  m_Clips:\n"
                + timeline_clip(0, audio_dur, apa_id, tid, dispname, audio=True)
                + "  m_Markers:\n    m_Objects: []\n"
                + "  m_TrackProperties:\n    volume: 1\n    stereoPan: 0\n    spatialBlend: 0\n")
        return body

    def audio_playable(apa_id, guid):
        return (mono_header(apa_id)
                + f"  m_Script: {{fileID: 11500000, guid: {SCRIPT_AUDIOPLAYABLE}, type: 3}}\n"
                + "  m_Name: AudioPlayableAsset\n"
                + "  m_EditorClassIdentifier: Unity.Timeline::UnityEngine.Timeline.AudioPlayableAsset\n"
                + f"  m_Clip: {{fileID: 8300000, guid: {guid}, type: 3}}\n"
                + "  m_Loop: 0\n"
                + "  m_bufferingTime: 0.1\n"
                + "  m_ClipProperties:\n    volume: 1\n")

    # main timeline asset
    tracks_list = "".join(
        f"  - {{fileID: {x}}}\n" for x in
        [back_track_id, acc_track_id] + [rhythm_track_ids[l] for l in range(4)]
    )
    main = (mono_header(11400000, hide=0)
            + f"  m_Script: {{fileID: 11500000, guid: {SCRIPT_TIMELINE}, type: 3}}\n"
            + f"  m_Name: {name}\n"
            + "  m_EditorClassIdentifier:\n"
            + "  m_Version: 0\n"
            + "  m_Tracks:\n" + tracks_list
            + "  m_FixedDuration: 0\n"
            + "  m_EditorSettings:\n    m_Framerate: 60\n    m_ScenePreview: 1\n"
            + "  m_DurationMode: 0\n"
            + "  m_MarkerTrack: {fileID: 0}\n"
            + f"  m_Bpm: {bpm:g}\n"
            + "  m_OverrideNoteSpeed: 0\n"
            + "  m_NoteSpeed: 3\n"
            + "  m_FullName:\n  m_Authour:\n  m_Description:\n"
            + "  m_Dificulty: 0\n"
            + "  m_AudioClip: {fileID: 0}\n"
            + "  m_StartScore: 0\n  m_MaxScore: -1\n  m_MinScore: 0\n"
            + "  m_PreventMaxScoreOvershoot: 0\n  m_PreventMinScoreOvershoot: 0\n"
            + "  m_HighScore:\n    m_NoteAccuracyIDHistogram:\n    m_FullScore: 0\n    m_MaxChain: 0\n")

    out_text = "%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n"
    out_text += main
    out_text += rhythm_track(0) + rhythm_track(1) + rhythm_track(2) + rhythm_track(3)
    out_text += audio_track(back_track_id, back_apa_id, "Audio Track", os.path.splitext(os.path.basename(back_wav))[0])
    out_text += audio_track(acc_track_id, acc_apa_id, "Audio Track (1)", os.path.splitext(os.path.basename(acc_wav))[0])
    out_text += audio_playable(back_apa_id, back_guid)
    out_text += audio_playable(acc_apa_id, acc_guid)
    out_text += "".join(rhythmclip_blocks)

    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write(out_text)

    per_lane = {l: len(clips_by_lane[l]) for l in range(4)}
    print(f"OK: {out}")
    print(f"   notas={len(notes)}  por lane={per_lane}  dur_audio={audio_dur:.2f}s  tap_dur={tap_dur:.4f}")


if __name__ == "__main__":
    main()
