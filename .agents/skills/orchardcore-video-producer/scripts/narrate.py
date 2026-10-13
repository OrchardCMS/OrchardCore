"""Generates the narration of a video: one mp3 per step of storyboard.json, with the Orchard Core voice, and
durations.json, which the recorder and the assembly read.

Usage: python narrate.py <workspace> [--force]

Only the steps whose text changed are generated again.
"""
import asyncio
import json
import re
import subprocess
import sys
from pathlib import Path

import imageio_ffmpeg

# The voice of every Orchard Core video. See references/specs.md.
VOICE = "en-US-AndrewMultilingualNeural"
RATE = "+2%"

# How the voice must say words it misreads. The subtitles keep the written form. A storyboard can add its own in
# "spoken"; add the ones that apply to every video here.
SPOKEN = {
    "ASP.NET": "A.S.P. .NET",
}


def spoken(text, overrides):
    for written, said in {**SPOKEN, **overrides}.items():
        text = text.replace(written, said)
    return text


def duration(path):
    result = subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(), "-hide_banner", "-i", str(path)], capture_output=True, text=True)
    h, m, s = re.search(r"Duration: (\d+):(\d+):([\d.]+)", result.stderr).groups()
    return int(h) * 3600 + int(m) * 60 + float(s)


async def synthesize(text, path):
    import edge_tts
    for attempt in range(4):
        try:
            await edge_tts.Communicate(text, VOICE, rate=RATE).save(str(path))
            return
        except Exception:
            if attempt == 3:
                raise
            await asyncio.sleep(2)


async def main():
    workspace = Path(sys.argv[1] if len(sys.argv) > 1 and not sys.argv[1].startswith("--") else ".").resolve()
    force = "--force" in sys.argv
    storyboard = json.loads((workspace / "storyboard.json").read_text(encoding="utf-8"))
    overrides = storyboard.get("spoken", {})
    audio = workspace / "audio"
    audio.mkdir(exist_ok=True)
    durations = {}

    for chapter in storyboard["chapters"]:
        for clip in chapter["clips"]:
            for step in clip["steps"]:
                text = spoken(step["text"], overrides)
                path = audio / f"{step['id']}.mp3"
                stamp = audio / f"{step['id']}.txt"
                if force or not path.exists() or not stamp.exists() or stamp.read_text(encoding="utf-8") != text:
                    print("narrating", step["id"], flush=True)
                    await synthesize(text, path)
                    stamp.write_text(text, encoding="utf-8")
                durations[step["id"]] = round(duration(path), 2)

    (workspace / "durations.json").write_text(json.dumps(durations, indent=1), encoding="utf-8")
    print(f"{len(durations)} steps, {sum(durations.values()) / 60:.1f} minutes of narration")


if __name__ == "__main__":
    asyncio.run(main())
