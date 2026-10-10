"""Shows how the voice reads a phrase, to choose the spelling of a word it misreads: the words it speaks, when each
one starts, and the gap to the next one. Optionally writes the audio of each spelling to listen to.

Usage: python pronounce.py "<sentence with {}>" "<spelling>" ["<spelling>"...] [--save <folder>]

Example: python pronounce.py "It encrypts secrets with {} Core Data Protection." "ASP.NET" "A S P dot net" "A, S, P dot net"

Letters spelled out must be separate words, with gaps of about 0.4 to 0.8 seconds: "A S P" gives 0.25 second gaps,
and the letters run together into "asp"; "A, S, P" spaces them like a person spelling them. A single token such as
"A.S.P." can't be checked this way.
"""
import asyncio
import sys
from pathlib import Path

import edge_tts

from narrate import RATE, VOICE


async def probe(sentence, spelling, save):
    communicate = edge_tts.Communicate(sentence.format(spelling), VOICE, rate=RATE, boundary="WordBoundary")
    audio, words = b"", []
    async for chunk in communicate.stream():
        if chunk["type"] == "audio":
            audio += chunk["data"]
        elif chunk["type"] == "WordBoundary":
            words.append((chunk["text"], chunk["offset"] / 1e7))

    # The words of the spelling, and the word after it.
    before = len(sentence.split("{}")[0].split())
    spoken = words[before:before + len(spelling.split()) + 1]
    gaps = [round(b[1] - a[1], 2) for a, b in zip(spoken, spoken[1:])]
    print(f"{spelling!r:28} words {[w for w, _ in spoken[:-1]]}  gaps {gaps}  ends {words[-1][1]:.2f}s")

    if save:
        name = "".join(c if c.isalnum() else "_" for c in spelling)
        (save / f"{name}.mp3").write_bytes(audio)


async def main():
    args = sys.argv[1:]
    save = None
    if "--save" in args:
        index = args.index("--save")
        save = Path(args[index + 1])
        save.mkdir(parents=True, exist_ok=True)
        del args[index:index + 2]
    sentence, spellings = args[0], args[1:]
    for spelling in spellings:
        await probe(sentence, spelling, save)


asyncio.run(main())
