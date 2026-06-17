"""
Pipeline completo para adicionar as musicas novas ao Forro Hero.
Para cada musica: gera MIDIs (Easy/Hard), gera os 2 charts (.asset + .meta com GUID
novo), aplica o override de velocidade, e cria o SongData (.asset + .meta).
No fim reescreve o SongsData.asset com TODAS as musicas (Asa Branca + as novas).

Uso: python add_songs.py
"""
import os
import sys
import subprocess
import uuid

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
TOOLS = os.path.join(ROOT, "Tools")
NOVOS = os.path.join(ROOT, "Assets/ForroHero/Clients/UniFacisa/Audio/MUSICS/novos")
CHARTS = os.path.join(ROOT, "Assets/ForroHero/Clients/UniFacisa/Prefabs/Charts")
SONGS = os.path.join(ROOT, "Assets/ForroHero/Clients/UniFacisa/Prefabs/Data/Songs")
SONGSDATA = os.path.join(ROOT, "Assets/ForroHero/Clients/UniFacisa/Prefabs/Data/SongsData.asset")

SONGDATA_SCRIPT_GUID = "f7c0fcc1b6a674844aaebd219c703c4d"
SONGSDATA_SCRIPT_GUID = "e54b958955e9f9f4f85788e56fd4a69c"
ASA_BRANCA_GUID = "f9c458abdfb040b190877276deaf3a59"  # ja existe
LEADIN = "1.0"

# (pasta, prefixo, bpm, SongName, base_nome, songdata_nome)
TABLE = [
    ("Ai Que Saudade D'Ocê", "Ai-que-saudade", 104, "Ai, Que Saudade D'Ocê",
     "Ai-que-saudade-104bpm", "SongData-Ai-Que-Saudade"),
    ("Dengo", "Dengo", 72, "Dengo",
     "Dengo-72bpm", "SongData-Dengo"),
    ("Eu Só Quero Um Xodó", "Eu-so-quero-um-xodo", 94, "Eu Só Quero Um Xodó",
     "Eu-so-quero-um-xodo-94bpm", "SongData-Eu-So-Quero-Um-Xodo"),
    ("Numa Sala de Reboco", "Numa-sala-de-reboco", 153, "Numa Sala de Reboco",
     "Numa-sala-de-reboco-153bpm", "SongData-Numa-Sala-de-Reboco"),
    ("Tareco e Mariola", "Tareco-e-mariola", 85, "Tareco e Mariola",
     "Tareco-e-mariola-85bpm", "SongData-Tareco-e-Mariola"),
]

CHART_META = ("fileFormatVersion: 2\n"
              "guid: {guid}\n"
              "NativeFormatImporter:\n"
              "  externalObjects: {{}}\n"
              "  mainObjectFileID: 11400000\n"
              "  userData: \n"
              "  assetBundleName: \n"
              "  assetBundleVariant: \n")

SONGDATA_META = ("fileFormatVersion: 2\n"
                 "guid: {guid}\n"
                 "NativeFormatImporter:\n"
                 "  externalObjects: {{}}\n"
                 "  mainObjectFileID: 11400000\n"
                 "  userData:\n"
                 "  assetBundleName:\n"
                 "  assetBundleVariant:\n")

SONGDATA_ASSET = ("""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: 0}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {script_guid}, type: 3}}
  m_Name: {asset_name}
  m_EditorClassIdentifier:
  SongName: {song_name}
  BPM: {bpm}
  EasyTimeline: {{fileID: 11400000, guid: {easy_guid}, type: 2}}
  HardTimeline: {{fileID: 11400000, guid: {hard_guid}, type: 2}}
  AccordionAudioClip: {{fileID: 8300000, guid: {acc_guid}, type: 3}}
  PreviewAudioClip: {{fileID: 8300000, guid: {pre_guid}, type: 3}}
""")


def newguid():
    return uuid.uuid4().hex


def read_guid(meta_path):
    with open(meta_path, "r", encoding="utf-8") as f:
        for line in f:
            if line.startswith("guid:"):
                return line.split(":", 1)[1].strip()
    raise RuntimeError("guid nao encontrado: " + meta_path)


def write(path, text):
    with open(path, "w", encoding="utf-8", newline="\n") as f:
        f.write(text)


def apply_speed_override(asset_path):
    s = open(asset_path, encoding="utf-8").read()
    s = s.replace("  m_OverrideNoteSpeed: 0\n  m_NoteSpeed: 3\n",
                  "  m_OverrideNoteSpeed: 1\n  m_NoteSpeed: 9\n")
    write(asset_path, s)


def main():
    py = sys.executable
    new_songdata_guids = []

    for folder, prefix, bpm, song_name, base, sd_name in TABLE:
        sdir = os.path.join(NOVOS, folder)
        acc_wav = os.path.join(sdir, f"{prefix}-Accordion-{bpm}bpm.wav")
        back_wav = os.path.join(sdir, f"{prefix}-Back-{bpm}bpm.wav")
        out_base = os.path.join(sdir, f"{prefix}-{bpm}bpm")
        print(f"\n=== {folder} ({bpm} BPM) ===")

        # 1) MIDIs
        subprocess.run([py, os.path.join(TOOLS, "audio_to_chart.py"),
                        acc_wav, str(bpm), out_base], check=True)

        # 2) charts Easy/Hard
        chart_guids = {}
        for diff in ("Easy", "Hard"):
            midi = f"{out_base}-{diff}.mid"
            asset = os.path.join(CHARTS, f"{base}-{diff}_Timeline.asset")
            subprocess.run([py, os.path.join(TOOLS, "gen_chart.py"),
                            midi, str(bpm), back_wav, acc_wav, asset, LEADIN], check=True)
            apply_speed_override(asset)
            guid = newguid()
            write(asset + ".meta", CHART_META.format(guid=guid))
            chart_guids[diff] = guid

        # 3) SongData (.asset + .meta)
        acc_guid = read_guid(acc_wav + ".meta")
        pre_guid = read_guid(os.path.join(sdir, f"{prefix}-Preview-{bpm}bpm.wav") + ".meta")
        sd_asset = os.path.join(SONGS, sd_name + ".asset")
        write(sd_asset, SONGDATA_ASSET.format(
            script_guid=SONGDATA_SCRIPT_GUID, asset_name=sd_name, song_name=song_name,
            bpm=bpm, easy_guid=chart_guids["Easy"], hard_guid=chart_guids["Hard"],
            acc_guid=acc_guid, pre_guid=pre_guid))
        sd_guid = newguid()
        write(sd_asset + ".meta", SONGDATA_META.format(guid=sd_guid))
        new_songdata_guids.append(sd_guid)
        print(f"   SongData {sd_name}  guid={sd_guid}")

    # 4) reescreve SongsData com Asa Branca + as 5 novas
    all_guids = [ASA_BRANCA_GUID] + new_songdata_guids
    songs_lines = "".join(
        f"  - {{fileID: 11400000, guid: {g}, type: 2}}\n" for g in all_guids)
    songsdata = ("""%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!114 &11400000
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 0}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: """ + SONGSDATA_SCRIPT_GUID + """, type: 3}
  m_Name: SongsData
  m_EditorClassIdentifier:
  Songs:
""" + songs_lines)
    write(SONGSDATA, songsdata)
    print(f"\nSongsData reescrito com {len(all_guids)} musicas.")


if __name__ == "__main__":
    main()
