"""Writes a contact sheet of a recorded clip, to check it without watching it: the frame 2 seconds into each step,
the frame at the end of its narration, and the frames at the given times.

Usage: python review.py <workspace> <clip> [seconds...]

Writes shots/<clip>.jpg in the workspace.
"""
import json
import sys
from pathlib import Path

from PIL import Image, ImageDraw

workspace = Path(sys.argv[1]).resolve()
clip = sys.argv[2]
extra = [float(t) for t in sys.argv[3:]]
directory = workspace / "clips" / clip
timeline = json.loads((directory / "timeline.json").read_text())
durations = json.loads((workspace / "durations.json").read_text())
frames = timeline["frames"]


def at(t):
    best = frames[0]
    for item in frames:
        if item["t"] <= t:
            best = item
    return best["file"]


times = []
for step in timeline["steps"]:
    times.append((f"{step['id']} +2s", step["t"] + 2))
    times.append((f"{step['id']} end", step["t"] + durations.get(step["id"], 2)))
times += [(f"t={t}", t) for t in extra]

thumbs = []
for label, t in times:
    image = Image.open(directory / "frames" / at(t)).convert("RGB")
    image.thumbnail((768, 432))
    canvas = Image.new("RGB", (768, 460), "white")
    canvas.paste(image, (0, 28))
    ImageDraw.Draw(canvas).text((6, 6), label, fill="black")
    thumbs.append(canvas)

columns = 2
sheet = Image.new("RGB", (768 * columns, 460 * ((len(thumbs) + columns - 1) // columns)), "white")
for index, thumb in enumerate(thumbs):
    sheet.paste(thumb, ((index % columns) * 768, (index // columns) * 460))
out = workspace / "shots" / f"{clip}.jpg"
out.parent.mkdir(exist_ok=True)
sheet.save(out, quality=80)
print(out)
