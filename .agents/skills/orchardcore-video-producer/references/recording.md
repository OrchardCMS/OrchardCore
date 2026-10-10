# Recording

## The site to record

Record a fresh, throwaway site, never a site with real data:

- Run `src/OrchardCore.Cms.Web` on a free port (5010 by default, `VIDEO_BASE_URL` otherwise), with its own
  `App_Data` folder in the workspace (`ORCHARD_APP_DATA`), provisioned by Auto Setup through environment variables, so
  no setup or login form is recorded. See the `orchardcore-tester` skill (`references/autosetup.md`). Use the test
  credentials `admin` / `admin@test.com` / `Password1!` (`VIDEO_USER_NAME`, `VIDEO_PASSWORD` otherwise).
- Name the site "Orchard Core", in the `America/Los_Angeles` time zone, with the Blog recipe unless the subject
  needs another.
- Use realistic but fake data: `Payments.ApiKey`, `noreply@orchardcore.net`, `+14257041234`, `sk_live_51HxQ2`.
- When the subject needs an external service (a vault, a mail server), use a local emulator in Docker, and a
  throwaway host project outside the solution that registers it. Accept an emulator's self-signed certificate only
  in that process, never in the machine's trust store. Remove the container and the project afterwards.

### Snapshots

Clips run in order, each one starting from the state the previous one left. Before recording, put the site in the
starting state (features enabled, settings saved) with a `prepare` command that you add to the recorder's `Program.cs`, then copy
its `App_Data` folder and `state.json` aside. To re-record everything, stop the site, restore the copy, and start
it again. To re-record one clip, make sure the site is in the state that clip expects.

## The recorder

`scripts/recorder` is a Playwright for .NET console app. Copy it into the workspace, add your clips to `Scenes.cs`,
and build it there (its output goes to a short temp path, as Playwright's files exceed `MAX_PATH` under deep folders
on Windows).

```bash
export VIDEO_ROOT=<workspace>           # where state.json, durations.json and clips/ are
export VIDEO_BASE_URL=http://localhost:5010
Recorder setup                          # logs in, and saves the session in state.json
Recorder probe /Admin/Features "() => document.title"   # inspects a page (PROBE_SHOT=x.png saves a screenshot)
Recorder record all                     # or: Recorder record 03-create 04-manage
```

In Git Bash on Windows, set `MSYS_NO_PATHCONV=1`, or paths such as `/Admin/Features` are turned into Windows paths.

## The scene API (`Scene.cs`)

A clip method creates a scene, prepares the page, starts the capture, and runs one step per narrated step:

```csharp
await using var scene = await NewSceneAsync(browser, directory, durations);
await scene.OpenAsync("/Admin/Secrets/Index");   // not recorded
await scene.StartCaptureAsync();                  // recording starts

await scene.StepAsync("create-1", async () =>
{
    await scene.ClickAsync(scene.Page.Locator("button.create"), pauseAfter: 900);
});
```

| Method | Use |
|---|---|
| `OpenAsync(path)` | Opens a page before the capture starts. |
| `StartCaptureAsync()` | Starts recording, after a still moment. |
| `StepAsync(id, actions)` | Marks the start of a step, runs its actions, and holds until its narration ends. Keep the actions shorter than the narration. |
| `MoveToAsync(locator, offsetX, offsetY)` | Moves the cursor smoothly to an element. |
| `ClickAsync(locator)` / `DoubleClickAsync` / `RightClickAsync` | Moves, then clicks, with a green ripple. |
| `ClickAndWaitForNavigationAsync(locator)` | Clicks something that loads a new page, and keeps capturing it. |
| `WaitForNavigationAsync(action)` | Runs an action that loads a new page (a key press, a form submit). |
| `NavigateAsync(path)` | Goes to a page during the capture. |
| `TypeAsync(locator, text, delay)` | Clicks a field, and types into it, visibly. |
| `ScrollAsync(dx, dy)` / `DragAsync(locator, x, y)` | Scrolls, drags. |
| `HighlightAsync(locator, caption)` / `ClearHighlightsAsync()` | Draws a green ring, with an optional caption, around what the narration talks about. |
| `SwitchToAsync(page)` | Records another tab, such as one opened by a link with `target="_blank"`. |
| `PauseAsync(ms)` / `HoldAsync(seconds)` | Waits, while frames keep coming. |

## Pitfalls

- **Narrate first.** The recorder reads `durations.json` to hold each step; recording before narrating gives clips
  whose steps are too short. If the narration of a step grows afterwards, record its clip again, unless it still
  ends before the next step starts.
- **Navigation stops the screencast**: always navigate with `ClickAndWaitForNavigationAsync`,
  `WaitForNavigationAsync` or `NavigateAsync`, or the rest of the clip is frozen.
- **Expand menus before `StartCaptureAsync`** when opening them isn't part of the story.
- **Seed data outside the capture**, with `fetch` calls in the page, rather than by clicking through forms on camera.
- **Highlights are fixed-position**: clear them before scrolling.
- **Look at every contact sheet** (`review.py`): an error message or an empty list is easy to miss in the logs.
- **Rebuild the site** after changing the code under it, and stop it before building the solution (it locks DLLs).
