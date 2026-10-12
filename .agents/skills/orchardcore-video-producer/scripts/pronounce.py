"""Makes a short pronunciation sample: one labeled option of about 5 seconds per spelling of a word, each saying the
same line with the video's voice. A person listens to it and picks the option that sounds right, before any narration
is generated with it. Also prints the words the voice speaks for each spelling.

Usage: python pronounce.py <output.mp4> "<line with {}>" "<written word>" "<spelling>" ["<spelling>"...]

Example:
    python pronounce.py asp-net.mp4 "It encrypts secrets with {} Core Data Protection." "ASP.NET" "A.S.P. .NET" "A-S-P dot net" "A S P dot net"

Each option shows the line with the written word, as in the captions.
"""
import asyncio
import subprocess
import sys
import tempfile
from pathlib import Path

import edge_tts
import imageio_ffmpeg

import brand
from narrate import RATE, VOICE

FFMPEG = imageio_ffmpeg.get_ffmpeg_exe()


async def option(directory, index, line, spelling, caption):
    mp3 = directory / f"{index}.mp3"
    communicate = edge_tts.Communicate(line.format(spelling), VOICE, rate=RATE, boundary="WordBoundary")
    audio, words = b"", []
    async for chunk in communicate.stream():
        if chunk["type"] == "audio":
            audio += chunk["data"]
        elif chunk["type"] == "WordBoundary":
            words.append(chunk["text"])
    mp3.write_bytes(audio)
    print(f"Option {index}: {spelling!r} is spoken as {words}")

    png = directory / f"{index}.png"
    brand.card(f"Option {index}", caption, label="Pronunciation check").save(png)
    mp4 = directory / f"{index}.mp4"
    subprocess.run([FFMPEG, "-y", "-loglevel", "error", "-loop", "1", "-framerate", str(brand.FPS), "-i", str(png), "-i", str(mp3),
                    "-filter_complex", "[1:a]adelay=500|500,apad=pad_dur=0.8[a]", "-map", "0:v", "-map", "[a]", "-shortest",
                    "-c:v", "libx264", "-tune", "stillimage", "-crf", "28", "-pix_fmt", "yuv420p",
                    "-c:a", "aac", "-b:a", "96k", "-ar", "48000", "-ac", "1", str(mp4)], check=True)
    return mp4


async def main():
    output, line, written, spellings = Path(sys.argv[1]).resolve(), sys.argv[2], sys.argv[3], sys.argv[4:]
    caption = line.format(written)
    with tempfile.TemporaryDirectory() as temp:
        directory = Path(temp)
        parts = [await option(directory, index, line, spelling, caption) for index, spelling in enumerate(spellings, 1)]
        concat = directory / "list.txt"
        concat.write_text("".join(f"file '{part.as_posix()}'\n" for part in parts), encoding="utf-8")
        subprocess.run([FFMPEG, "-y", "-loglevel", "error", "-f", "concat", "-safe", "0", "-i", str(concat), "-c", "copy", str(output)], check=True)
    print(output)


asyncio.run(main())
