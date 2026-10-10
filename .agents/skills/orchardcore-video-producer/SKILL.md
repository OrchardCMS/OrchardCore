---
name: orchardcore-video-producer
description: Produces narrated Orchard Core videos (feature walkthroughs, module overviews, pull request demos) with one unified look and specification - 1080p, Orchard Core logo, colors and fonts, the same neural voice, captions, and an encode under 10 MB. Use when asked to record, narrate, or produce a video or screencast of Orchard Core, or to add one to the docs or a pull request.
---

# Orchard Core Video Producer

Every Orchard Core video looks and sounds the same: the same resolution, branding, layout, voice, captions, and
encoding. This skill holds that specification and the scripts that apply it. **Don't change the specification for a
single video.** If it must change, change it here, in `scripts/brand.py`, `scripts/narrate.py` and
`scripts/assemble.py`, and in `references/specs.md`, so every later video follows.

The worked example in `examples/secrets-module/` is the storyboard and slides of the Secrets module video, published
with the documentation of the Secrets module. Read it before writing a new one.

## The specification, in short

| | |
|---|---|
| Output | 1920x1080, 30 fps, H.264 (`-preset slow -crf 33 -tune stillimage`), AAC 64 kbps mono, `+faststart`, **under 10 MB** |
| Recording | Chromium at 1536x864 CSS pixels, device scale 1.25, light color scheme, en-US, shown at 1728x972 in a green frame |
| Branding | Dark background with green glows, Orchard Core logo (white name, green symbol), Open Sans from `src/docs/reference/branding/assets/fonts` |
| Colors | Green `#41B670` (accents, frame, highlights, cursor clicks), dark green `#1D5B36`, charcoal `#404041`, gray `#C6C6C6`, white |
| Voice | `en-US-AndrewMultilingualNeural`, rate `+2%`, through `edge-tts` |
| Structure | Opening card, numbered chapters (chapter card + slides or recordings), closing card |
| Captions | WebVTT and SRT, from the narration, at most about 13 words each |

The full specification, with every value and the reasons behind them, is in `references/specs.md`.

## Prerequisites

- Python 3.11+ with `pip install pillow pygments imageio-ffmpeg edge-tts` (ffmpeg comes with `imageio-ffmpeg`).
- .NET 10 SDK, for the recorder (Playwright for .NET; run `pwsh <bin>/playwright.ps1 install chromium` once).
- A throwaway local Orchard Core site to record: see `references/recording.md`.
- Internet access for the voice (`edge-tts` calls the Microsoft Edge speech service).

## Workflow

### 1. Create a workspace outside the repository

Use the session's scratchpad or a temp folder, never the repository: frames and audio are large, and the recorder
must not pick up the repository's build settings.

```
<workspace>/
  storyboard.json   the chapters, clips and narration (see references/storyboard.md)
  slides.py         the slides, if the storyboard has "slide" clips
  recorder/         a copy of scripts/recorder, with your scenes in Scenes.cs
  audio/            generated: one mp3 per step
  durations.json    generated: the length of each step's narration
  clips/<clip>/     generated: the frames and timeline.json of each recording
  build/            generated: intermediate segments, and master.mp4
  <output>.mp4/.vtt/.srt   the result
```

Start from `examples/secrets-module/storyboard.json` and `slides.py`, and copy `scripts/recorder` into the workspace.

### 2. Write the storyboard

Read `references/storyboard.md` first. In short:

- Write for the long term: say "the X module", never "the new X", "now", "in this release", or version numbers, so
  the video stays accurate for years.
- One idea per step, one to three sentences, spoken in the present tense. A step is what the viewer sees while it plays.
- Chapters: why it exists, each feature, how to configure it, how to adopt or extend it.

### 3. Generate the narration

```bash
python .agents/skills/orchardcore-video-producer/scripts/narrate.py <workspace>
```

It writes `audio/` and `durations.json`. Run it again after any change to the text: only the changed steps are
generated again. **Record after narrating**, since the recorder holds each step for the length of its narration.

If the voice mispronounces a word, add it to `SPOKEN` in `scripts/narrate.py` (for every video) or to `"spoken"` in
the storyboard (for this one). The captions keep the written form. **Have a person confirm a new spelling before
using it**: make a pronunciation sample, a few seconds per candidate spelling, and ask which option sounds right.
Rebuilding a whole video to try a spelling wastes minutes per attempt.

```bash
python .agents/skills/orchardcore-video-producer/scripts/pronounce.py asp-net.mp4 "It encrypts secrets with {} Core Data Protection." "ASP.NET" "A.S.P. .NET" "A-S-P dot net" "A S P dot net"
```

Letters said one by one are written as an abbreviation with periods, such as "A.S.P.": the voice spells it at an
even pace. With spaces ("A S P"), the letters run together; with commas ("A, S, P"), the pauses are uneven.
"ASP.NET" is spoken as "A.S.P. .NET".

### 4. Record the clips

Write one method per "clip" in `recorder/Scenes.cs`, with one `scene.StepAsync("<step id>", ...)` per step, in the
storyboard's order. See `references/recording.md` for the scene API, the site to record, and the pitfalls. Then:

```bash
cd <workspace>/recorder && dotnet build
export VIDEO_ROOT=<workspace> VIDEO_BASE_URL=http://localhost:5010
<bin>/Recorder setup          # logs in, once
<bin>/Recorder record all     # or the names of some clips
python .agents/skills/orchardcore-video-producer/scripts/review.py <workspace> <clip>
```

Look at every contact sheet (`shots/<clip>.jpg`) before assembling: the frame at the end of each step must show what
its narration describes.

### 5. Assemble

```bash
python .agents/skills/orchardcore-video-producer/scripts/assemble.py <workspace>
```

It prints the length and size. If it's over 10 MB, shorten the video, or raise the CRF one step at a time
(`CRF=34 python .../assemble.py <workspace>`) and check that text stays readable. Extract a few frames with ffmpeg
and look at them: a recording, a slide with code, and a card.

### 6. Publish

- **Documentation**: put `<name>.mp4`, `<name>.vtt` and a poster (`build/<opening card>.png` saved as JPEG) in a
  `videos/` folder next to the page, and embed it under the introduction of the page:

  ```html
  <video controls preload="metadata" width="100%" poster="videos/<name>.jpg">
      <source src="videos/<name>.mp4" type="video/mp4">
      <track kind="captions" src="videos/<name>.vtt" srclang="en" label="English">
      Your browser does not support embedded videos. <a href="videos/<name>.mp4">Download the video</a>.
  </video>
  ```

  Introduce it with one sentence that says what it covers, without "new". Check the page with `mkdocs build`.
- **Pull request**: GitHub only accepts videos through its web uploader (drag and drop, or the attachment button
  of the description editor), up to 10 MB on free plans. The upload gives a `https://github.com/user-attachments/assets/...`
  URL: put it alone on a line near the top of the description, so GitHub shows a player.

### 7. Clean up

Stop the site you recorded, remove any emulator containers and throwaway projects, and keep the workspace until the
video is accepted, in case a step must be re-recorded.

## References

- `references/specs.md`: the full specification.
- `references/storyboard.md`: the storyboard format, and how to write the narration and the slides.
- `references/recording.md`: the site to record, the recorder, the scene API, and pitfalls.
- `scripts/brand.py`: the branding, the layout, and the slide building blocks.
- `scripts/pronounce.py`: a pronunciation sample, to confirm the spelling of a word the voice misreads.
- `examples/secrets-module/`: a complete storyboard and slides.
