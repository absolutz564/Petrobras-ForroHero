"""
Reduz ~12% a densidade do Hard das musicas indicadas, achando por varredura o
gap que da ~88% da contagem atual (gap 0.42). Regera so o chart Hard.
"""
import os
import sys
import subprocess

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
sys.path.insert(0, os.path.join(ROOT, "Tools"))
import audio_to_chart as a2c

NOVOS = os.path.join(ROOT, "Assets/ForroHero/Clients/UniFacisa/Audio/MUSICS/novos")
CHARTS = os.path.join(ROOT, "Assets/ForroHero/Clients/UniFacisa/Prefabs/Charts")

# (pasta, prefixo, bpm)
SONGS = [
    ("Ai Que Saudade D'Ocê", "Ai-que-saudade", 104),
    ("Dengo", "Dengo", 72),
    ("Eu Só Quero Um Xodó", "Eu-so-quero-um-xodo", 94),
    ("Numa Sala de Reboco", "Numa-sala-de-reboco", 153),
]
BASE_GAP = 0.42
TARGET = 0.88  # 12% menos


def main():
    py = sys.executable
    for folder, prefix, bpm in SONGS:
        sdir = os.path.join(NOVOS, folder)
        acc = os.path.join(sdir, f"{prefix}-Accordion-{bpm}bpm.wav")
        back = os.path.join(sdir, f"{prefix}-Back-{bpm}bpm.wav")
        out_base = os.path.join(sdir, f"{prefix}-{bpm}bpm")
        chart = os.path.join(CHARTS, f"{prefix}-{bpm}bpm-Hard_Timeline.asset")

        onset_times, _, _, _, _ = a2c.detect(acc, bpm)
        beat = 60.0 / bpm
        base = len(a2c.thin(sorted(onset_times), beat * BASE_GAP))
        target = base * TARGET

        # varre o gap e escolhe o que da a contagem MAIS PROXIMA do alvo (12% menos)
        cands = []
        g = BASE_GAP
        while g <= 1.2:
            c = len(a2c.thin(sorted(onset_times), beat * g))
            cands.append((abs(c - target), g, c))
            g = round(g + 0.005, 3)
        _, gap, new_count = min(cands)
        pct = 100 * (1 - new_count / base)
        print(f"{folder:24s} base={base} -> {new_count} ({pct:.0f}% menos)  gap={gap}")

        # regera MIDIs (Easy fica igual) e o chart Hard
        subprocess.run([py, os.path.join(ROOT, "Tools/audio_to_chart.py"),
                        acc, str(bpm), out_base, str(gap)], check=True,
                       stdout=subprocess.DEVNULL)
        subprocess.run([py, os.path.join(ROOT, "Tools/gen_chart.py"),
                        f"{out_base}-Hard.mid", str(bpm), back, acc, chart, "1.0", "0.0"],
                       check=True, stdout=subprocess.DEVNULL)
        s = open(chart, encoding="utf-8").read()
        s = s.replace("  m_OverrideNoteSpeed: 0\n  m_NoteSpeed: 3\n",
                      "  m_OverrideNoteSpeed: 1\n  m_NoteSpeed: 9\n")
        open(chart, "w", encoding="utf-8", newline="\n").write(s)


if __name__ == "__main__":
    main()
