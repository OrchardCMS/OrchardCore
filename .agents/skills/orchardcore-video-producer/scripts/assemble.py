"""Assembles a video: title cards, slides, framed recordings, the narration, and captions.

Usage: python assemble.py <workspace>

Reads storyboard.json, durations.json, clips/<clip>/timeline.json and frames (from the recorder), and slides.py
(for "slide" clips). Writes <output>.mp4, <output>.vtt and <output>.srt in the workspace, where output is the
storyboard's "output". The intermediate files are in build/.
"""
import importlib.util
import json
import os
import re
import subprocess
import sys
from pathlib import Path

import imageio_ffmpeg

sys.path.insert(0, str(Path(__file__).resolve().parent))
import brand  # noqa: E402

FFMPEG = imageio_ffmpeg.get_ffmpeg_exe()
FPS = brand.FPS

# The published file. See references/specs.md: keep it under 10 MB so it can be attached to a GitHub pull request.
PUBLISH_CRF = os.environ.get("CRF", "33")
PUBLISH_AUDIO = "64k"

# Timing.
CHAPTER_CARD_SECONDS = 2.6
CARD_LEAD, CARD_TAIL = 1.0, 1.4
SLIDE_LEAD, SLIDE_GAP, SLIDE_TAIL = 0.6, 0.5, 1.0
CLIP_LEAD, CLIP_TAIL = 0.7, 1.0


def run(args):
    result = subprocess.run([FFMPEG, "-hide_banner", "-loglevel", "error", "-y", *args], capture_output=True, text=True)
    if result.returncode != 0:
        raise SystemExit(result.stderr)


def master_video():
    return ["-c:v", "libx264", "-preset", "medium", "-crf", "16", "-r", str(FPS), "-pix_fmt", "yuv420p"]


def master_audio():
    return ["-c:a", "aac", "-b:a", "192k", "-ar", "48000", "-ac", "2"]


class Assembler:
    def __init__(self, workspace):
        self.workspace = workspace
        self.build = workspace / "build"
        self.build.mkdir(exist_ok=True)
        self.storyboard = json.loads((workspace / "storyboard.json").read_text(encoding="utf-8"))
        self.durations = json.loads((workspace / "durations.json").read_text(encoding="utf-8"))
        self.slides = None
        if (workspace / "slides.py").exists():
            spec = importlib.util.spec_from_file_location("slides", workspace / "slides.py")
            self.slides = importlib.util.module_from_spec(spec)
            spec.loader.exec_module(self.slides)

    def audio(self, step_id):
        return str(self.workspace / "audio" / f"{step_id}.mp3")

    def still(self, image_path, seconds, sounds, output, fade_in=True, fade_out=True):
        """A still image with narration: a list of (mp3, offset in seconds)."""
        args = ["-loop", "1", "-framerate", str(FPS), "-t", f"{seconds:.3f}", "-i", str(image_path)]
        fades = (["fade=t=in:st=0:d=0.4"] if fade_in else []) + ([f"fade=t=out:st={seconds - 0.4:.3f}:d=0.4"] if fade_out else [])
        video = ",".join(fades + ["format=yuv420p"])
        if sounds:
            filters = []
            for index, (mp3, offset) in enumerate(sounds):
                args += ["-i", mp3]
                filters.append(f"[{index + 1}:a]adelay={int(offset * 1000)}|{int(offset * 1000)}[a{index}]")
            mix = "".join(f"[a{i}]" for i in range(len(sounds)))
            filters.append(f"{mix}amix=inputs={len(sounds)}:normalize=0,apad,atrim=0:{seconds:.3f}[aout]")
            filters.append(f"[0:v]{video}[vout]")
            args += ["-filter_complex", ";".join(filters), "-map", "[vout]", "-map", "[aout]"]
        else:
            args += ["-f", "lavfi", "-t", f"{seconds:.3f}", "-i", "anullsrc=r=48000:cl=stereo", "-vf", video, "-map", "0:v", "-map", "1:a"]
        run(args + master_video() + master_audio() + ["-t", f"{seconds:.3f}", str(output)])

    def recording(self, clip_id, chapter_title, label, steps, output):
        """A recording in the frame, with the narration of each step at the time the recorder started it."""
        directory = self.workspace / "clips" / clip_id
        timeline = json.loads((directory / "timeline.json").read_text())
        recorded = {step["id"]: step["t"] for step in timeline["steps"]}
        missing = [step["id"] for step in steps if step["id"] not in recorded]
        if missing:
            raise SystemExit(f"{clip_id} has no recording of {missing}")

        last = steps[-1]["id"]
        start = max(0.0, recorded[steps[0]["id"]] - CLIP_LEAD)
        narration_end = recorded[last] + self.durations[last]
        end = max(min(timeline["end"], narration_end + CLIP_TAIL), narration_end + 0.6)
        seconds = end - start

        # The frames as a concat list, each shown until the next one.
        frames = timeline["frames"]
        entries = []
        for index, item in enumerate(frames):
            next_t = frames[index + 1]["t"] if index + 1 < len(frames) else end
            a, b = max(item["t"], start), min(next_t, end)
            if b > a:
                entries.append((item["file"], b - a))
            elif item["t"] <= start < next_t:
                entries.append((item["file"], 0.001))
        if not entries:
            raise SystemExit(f"{clip_id}: no frames")
        concat = self.build / f"{clip_id}.frames.txt"
        with open(concat, "w", encoding="utf-8") as list_file:
            for name, length in entries:
                list_file.write(f"file '{(directory / 'frames' / name).as_posix()}'\nduration {length:.4f}\n")
            list_file.write(f"file '{(directory / 'frames' / entries[-1][0]).as_posix()}'\n")

        frame_png = self.build / f"{clip_id}.frame.png"
        brand.frame(chapter_title, label).save(frame_png)

        args = ["-loop", "1", "-framerate", str(FPS), "-t", f"{seconds:.3f}", "-i", str(frame_png),
                "-f", "concat", "-safe", "0", "-i", str(concat)]
        filters = [
            f"[1:v]scale={brand.VIDEO_W}:{brand.VIDEO_H}:flags=lanczos,fps={FPS},setpts=PTS-STARTPTS[rec]",
            f"[0:v][rec]overlay={brand.VIDEO_X}:{brand.VIDEO_Y}:shortest=0,trim=0:{seconds:.3f},"
            f"fade=t=in:st=0:d=0.35,fade=t=out:st={seconds - 0.35:.3f}:d=0.35,format=yuv420p[vout]",
        ]
        offsets = []
        for index, step in enumerate(steps):
            offset = recorded[step["id"]] - start
            args += ["-i", self.audio(step["id"])]
            filters.append(f"[{index + 2}:a]adelay={int(offset * 1000)}|{int(offset * 1000)}[a{index}]")
            offsets.append((step, offset))
        mix = "".join(f"[a{i}]" for i in range(len(steps)))
        filters.append(f"{mix}amix=inputs={len(steps)}:normalize=0,apad,atrim=0:{seconds:.3f}[aout]")
        args += ["-filter_complex", ";".join(filters), "-map", "[vout]", "-map", "[aout]"]
        run(args + master_video() + master_audio() + ["-t", f"{seconds:.3f}", str(output)])
        return seconds, offsets

    def run(self):
        segments, captions, clock = [], [], 0.0

        def caption(text, start, length):
            captions.extend((clock + a, clock + b, c) for a, b, c in chunks(text, start, length))

        for chapter in self.storyboard["chapters"]:
            title, subtitle = chapter["title"], chapter["subtitle"]
            # Chapters made only of cards, the opening and the closing, have no chapter card.
            numbered = any(clip["kind"] != "card" for clip in chapter["clips"])
            label = f"Chapter {int(chapter['id'])}" if numbered else None
            if numbered:
                png, path = self.build / f"card-{chapter['id']}.png", self.build / f"card-{chapter['id']}.mp4"
                brand.card(title, subtitle, label=label).save(png)
                self.still(png, CHAPTER_CARD_SECONDS, [], path)
                segments.append(path)
                clock += CHAPTER_CARD_SECONDS

            for clip in chapter["clips"]:
                print("encoding", clip["id"], flush=True)
                steps = clip["steps"]
                if clip["kind"] == "card":
                    png, path = self.build / f"{clip['id']}.png", self.build / f"{clip['id']}.mp4"
                    brand.card(title, subtitle, big=True).save(png)
                    step = steps[0]
                    seconds = CARD_LEAD + self.durations[step["id"]] + CARD_TAIL
                    self.still(png, seconds, [(self.audio(step["id"]), CARD_LEAD)], path)
                    caption(step["text"], CARD_LEAD, self.durations[step["id"]])
                    segments.append(path)
                    clock += seconds
                elif clip["kind"] == "slide":
                    if self.slides is None:
                        raise SystemExit(f"{clip['id']} is a slide clip, but the workspace has no slides.py")
                    for index, step in enumerate(steps):
                        first, last = index == 0, index == len(steps) - 1
                        png, path = self.build / f"slide-{step['id']}.png", self.build / f"slide-{step['id']}.mp4"
                        self.slides.slide(step["id"], title, label).save(png)
                        lead = SLIDE_LEAD if first else SLIDE_GAP / 2
                        seconds = lead + self.durations[step["id"]] + (SLIDE_TAIL if last else SLIDE_GAP)
                        self.still(png, seconds, [(self.audio(step["id"]), lead)], path, fade_in=first, fade_out=last)
                        caption(step["text"], lead, self.durations[step["id"]])
                        segments.append(path)
                        clock += seconds
                else:
                    path = self.build / f"{clip['id']}.mp4"
                    seconds, offsets = self.recording(clip["id"], title, label, steps, path)
                    for step, offset in offsets:
                        caption(step["text"], offset, self.durations[step["id"]])
                    segments.append(path)
                    clock += seconds

        concat = self.build / "segments.txt"
        concat.write_text("".join(f"file '{path.as_posix()}'\n" for path in segments), encoding="utf-8")
        master = self.build / "master.mp4"
        run(["-f", "concat", "-safe", "0", "-i", str(concat), "-c", "copy", str(master)])

        output = self.workspace / f"{self.storyboard['output']}.mp4"
        print("compressing", flush=True)
        publish(master, output)

        base = output.with_suffix("")
        with open(f"{base}.srt", "w", encoding="utf-8") as srt:
            for index, (a, b, text) in enumerate(captions, 1):
                srt.write(f"{index}\n{timestamp(a)} --> {timestamp(b)}\n{text}\n\n")
        with open(f"{base}.vtt", "w", encoding="utf-8") as vtt:
            vtt.write("WEBVTT\n\n")
            for a, b, text in captions:
                vtt.write(f"{timestamp(a, '.')} --> {timestamp(b, '.')}\n{text}\n\n")

        size = output.stat().st_size / 1_000_000
        print(f"{output}: {clock / 60:.1f} minutes, {len(segments)} segments, {size:.1f} MB")
        if size >= 10:
            print("Over 10 MB: raise CRF (CRF=34 python assemble.py ...), or shorten the video.")


def publish(master, output):
    """The published encode: H.264 at the publish CRF, tuned for screen content, AAC mono speech, fast start."""
    run(["-i", str(master), "-c:v", "libx264", "-preset", "slow", "-crf", PUBLISH_CRF, "-tune", "stillimage",
         "-pix_fmt", "yuv420p", "-r", str(FPS), "-g", str(FPS * 10),
         "-c:a", "aac", "-b:a", PUBLISH_AUDIO, "-ar", "48000", "-ac", "1", "-movflags", "+faststart", str(output)])


def timestamp(seconds, separator=","):
    ms = int(round(seconds * 1000))
    return f"{ms // 3600000:02}:{ms // 60000 % 60:02}:{ms // 1000 % 60:02}{separator}{ms % 1000:03}"


def chunks(text, start, length):
    """Splits a narration into captions of at most about 13 words, timed by their share of the characters."""
    parts = []
    for sentence in re.split(r"(?<=[.!?:])\s+", text):
        words = sentence.split()
        while words:
            take, words = words[:13], words[13:]
            if len(words) < 4:
                take, words = take + words, []
            parts.append(" ".join(take))
    total = sum(len(part) + 6 for part in parts)
    t = start
    for part in parts:
        share = length * (len(part) + 6) / total
        yield t, t + share, part
        t += share


if __name__ == "__main__":
    Assembler(Path(sys.argv[1] if len(sys.argv) > 1 else ".").resolve()).run()
