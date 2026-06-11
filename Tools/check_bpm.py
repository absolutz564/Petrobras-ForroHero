import sys, numpy as np, librosa

for path in sys.argv[1:]:
    y, sr = librosa.load(path, mono=True)
    # estimativa global (pode reportar metade/dobro)
    try:
        tempo = float(librosa.feature.rhythm.tempo(y=y, sr=sr)[0])
    except AttributeError:
        try:
            tempo = float(librosa.feature.tempo(y=y, sr=sr)[0])
        except AttributeError:
            tempo = float(librosa.beat.tempo(y=y, sr=sr)[0])
    # estimativa via beat tracker
    bt, _ = librosa.beat.beat_track(y=y, sr=sr)
    bt = float(np.atleast_1d(bt)[0])
    # candidatos com normalizacao para faixa 70-180
    def norm(b):
        while b < 70: b *= 2
        while b > 180: b /= 2
        return b
    name = path.split("/")[-1]
    print(f"{name}")
    print(f"   tempo(global)={tempo:.1f}  beat_track={bt:.1f}  | normalizados: {norm(tempo):.1f} / {norm(bt):.1f}")
