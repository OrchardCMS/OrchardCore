# The storyboard

`storyboard.json` describes the whole video: its chapters, the clips of each chapter, and the narrated steps of each
clip. `narrate.py`, the recorder and `assemble.py` all read it.

```json
{
  "title": "The Orchard Core Secrets module",
  "output": "secrets-module",
  "spoken": { "YesSql": "Yes S Q L" },
  "chapters": [
    {
      "id": "00", "title": "The Orchard Core Secrets module", "subtitle": "One secure home for the sensitive values of your site",
      "clips": [ { "id": "00-intro", "kind": "card", "steps": [ { "id": "intro", "text": "..." } ] } ]
    },
    {
      "id": "01", "title": "Why the Secrets module", "subtitle": "The problem it solves",
      "clips": [ { "id": "01-why", "kind": "slide", "steps": [ { "id": "why-1", "text": "..." } ] } ]
    },
    {
      "id": "02", "title": "Three features", "subtitle": "Secrets, Azure Key Vault, and OpenID Connect",
      "clips": [ { "id": "02-features", "kind": "clip", "steps": [ { "id": "features-1", "text": "..." } ] } ]
    }
  ]
}
```

- `output`: the file name of the result, without extension: `<output>.mp4`, `.vtt`, `.srt`.
- `spoken`: pronunciation fixes for this video only (see `SPOKEN` in `scripts/narrate.py` for those of every video).
- Chapter `id`: two digits. Chapters made only of `card` clips (the opening `00` and the closing `99`) get no
  chapter card; every other chapter is numbered "Chapter N" from its id.
- Clip `kind`:
  - `card`: the opening or closing card, with the chapter's title and subtitle, and one narrated step.
  - `slide`: one image per step, drawn by `slide(step_id, chapter_title, label)` in the workspace's `slides.py`.
  - `clip`: a recording, made by the method of the same name in `recorder/Scenes.cs`.
- Step `id`: unique in the video; the recorder marks the time each one starts.

## Structure

A module or feature overview usually follows this outline. Keep it between 4 and 9 minutes.

1. **Opening card** (`00`): what the subject is, in one or two sentences, and what the video covers.
2. **Why** (slides): the problem it solves, then how it solves it.
3. **The features** (recording): where they are, what each one does.
4. **Using it** (recordings): one chapter per task, in the order a user would do them.
5. **Configuring it** (slides with code): settings, `appsettings.json`, host registration, recipes.
6. **Extending or adopting it** (slides with code): the few steps a developer follows, with real API names.
7. **Closing card** (`99`): one sentence that sums it up, then "Thanks for watching."; the subtitle is
   `docs.orchardcore.net`.

A pull request demo can be shorter: an opening card, the recordings, and a closing card.

## Writing the narration

- **Timeless.** Say "the Secrets module", never "the new Secrets module", "now", "this release", "we added", or a
  version number. Describe what is, not what changed, so the video stays accurate for years.
- **One idea per step**, one to three sentences, 3 to 16 seconds. The viewer sees the step's actions while it plays,
  so describe what's on screen, as it happens.
- **Plain, direct, present tense.** Address the viewer as "you", or use "let's" for actions in a recording.
- **Name UI elements as they're labeled** on screen, so the viewer can find them: "Tools, Security, Secrets".
- **Acronyms**: the voice reads most of them correctly (API, SMTP, SMS, RSA). For one that must be spelled out letter by
  letter, add a `spoken` entry that writes it as an abbreviation with periods, such as "A.S.P. .NET" for "ASP.NET",
  and have a person confirm it with a sample from `scripts/pronounce.py`.
- **Spell out** what a voice could misread: say "Get Secret Value Async" for `GetSecretValueAsync()`, "an X.509
  certificate", "Azure AI Search". Check every name of the narration for pronunciation, and add fixes to `spoken`.
- **No marketing words**: no "powerful", "seamless", "simply", "easily".
- Avoid numbers that change, such as counts of features or settings, unless the video shows them.

## Writing the slides

`slides.py` exposes `slide(step_id, chapter_title, label)`, which returns a 1920x1080 PIL image built with the
helpers of `scripts/brand.py` (`assemble.py` puts it on the import path):

```python
from brand import W, bullets, draw_code, note_cards, slide_base

def slide(step_id, chapter_title, label):
    if step_id == "why-1":
        image = slide_base(chapter_title, label, "Without a central place for credentials")
        bullets(image, PROBLEMS, active={0, 1})   # the cards of this step are bright, the others dimmed
        return image
    if step_id == "adopt-1":
        image = slide_base(chapter_title, label, "1. Keep the value, add a secret name")
        draw_code(image, (96, 240, W - 96, 1030), "MyServiceSettings.cs", "csharp", SETTINGS, types=TYPES, size=28)
        return image
    raise SystemExit(f"No slide for {step_id}")
```

- Use only the helpers of `brand.py` and its colors and fonts: don't introduce new colors, fonts or layouts.
- `bullets`: up to four cards of (icon, title, text); icon is a short text such as "1", or `"check"`. Spread the
  cards over several steps with `active` to follow the narration.
- `draw_code`: real, compilable code from the documentation, trimmed to what the narration talks about; mark what's
  omitted with `// ...`. Pass the type names to `types`, so they are colored as types.
- `note_cards`: up to four (title, text) cards next to a code panel.
- Keep every text at 21 px or more, and render each slide once (`python -c "import slides; slides.slide('why-1', 'T', 'Chapter 1').save('x.png')"`
  from the workspace, with the skill's `scripts` folder on `PYTHONPATH`) to check nothing overflows.
