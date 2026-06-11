import re, sys
for fn in sys.argv[1:]:
    txt = open(fn, encoding="utf-8").read()
    anchors = set(re.findall(r'^--- !u!\d+ &(-?\d+)', txt, re.M))
    local_refs = set(re.findall(r'\{fileID: (-?\d+)\}', txt))
    clips = txt.count("m_RhythmPlayableBehaviour")
    rtracks = len(re.findall(r'm_Name: Rhythm Track', txt))
    atracks = txt.count("UnityEngine.Timeline.AudioTrack")
    apa = txt.count("m_Name: AudioPlayableAsset")
    ids = re.findall(r'^  m_ID: (\d)$', txt, re.M)
    tracks_in_list = re.findall(r'^  - \{fileID: (-?\d+)\}', txt, re.M)
    dangling = [r for r in local_refs if r != "0" and r not in anchors]
    print(fn.split("/")[-1] + ":")
    print(f"  rhythmclips={clips} rhythmtracks={rtracks} audiotracks={atracks} audioplayable={apa} lane_ids={ids}")
    print(f"  tracks_na_lista={len(tracks_in_list)} zeros={tracks_in_list.count('0')}")
    print(f"  refs_quebradas={dangling[:5]} (total {len(dangling)})")
