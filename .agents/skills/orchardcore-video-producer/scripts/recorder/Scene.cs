using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// Records one scene of a video: a browser page captured frame by frame through the DevTools screencast, with a
/// visible cursor, and the start time of each narrated step, so the narration can be laid over the video.
/// </summary>
public sealed class Scene : IAsyncDisposable
{
    public const int Width = 1536;
    public const int Height = 864;
    public const float Scale = 1.25f;

    // The cursor and the highlight ring, drawn in the page since a headless browser draws no cursor.
    private const string OverlayScript = """
        (() => {
          if (window.__demoOverlay) return;
          window.__demoOverlay = true;
          const install = () => {
            if (document.getElementById('demo-cursor')) return;
            const style = document.createElement('style');
            style.textContent = `
              #demo-cursor { position: fixed; left: 0; top: 0; width: 26px; height: 26px; z-index: 2147483647; pointer-events: none;
                transform: translate(var(--x, -100px), var(--y, -100px)); transition: none; }
              #demo-cursor svg { filter: drop-shadow(0 2px 3px rgba(0,0,0,.45)); }
              .demo-ripple { position: fixed; z-index: 2147483646; pointer-events: none; width: 34px; height: 34px; margin: -17px 0 0 -17px;
                border-radius: 50%; border: 3px solid #41B670; background: rgba(65,182,112,.25); animation: demo-ripple .6s ease-out forwards; }
              @keyframes demo-ripple { from { transform: scale(.3); opacity: 1 } to { transform: scale(1.6); opacity: 0 } }
              .demo-highlight { position: fixed; z-index: 2147483645; pointer-events: none; border: 3px solid #41B670; border-radius: 10px;
                box-shadow: 0 0 0 4px rgba(65,182,112,.25), 0 0 24px rgba(65,182,112,.55); transition: opacity .3s; }
              .demo-caption { position: fixed; z-index: 2147483645; pointer-events: none; background: #41B670; color: #ffffff; font: 600 15px/1.3 system-ui, sans-serif;
                padding: 6px 10px; border-radius: 6px; box-shadow: 0 4px 14px rgba(0,0,0,.35); max-width: 360px; }`;
            document.documentElement.appendChild(style);
            const cursor = document.createElement('div');
            cursor.id = 'demo-cursor';
            cursor.innerHTML = '<svg width="26" height="26" viewBox="0 0 26 26"><path d="M3 2 L3 21 L8.5 16 L12.5 24.5 L16 23 L12 14.8 L19.5 14.8 Z" fill="#fff" stroke="#1a1a1d" stroke-width="1.6" stroke-linejoin="round"/></svg>';
            document.documentElement.appendChild(cursor);
            const saved = sessionStorage.getItem('demo-cursor');
            if (saved) { const [x, y] = saved.split(','); cursor.style.setProperty('--x', x + 'px'); cursor.style.setProperty('--y', y + 'px'); }
          };
          document.addEventListener('mousemove', (e) => {
            install();
            const cursor = document.getElementById('demo-cursor');
            cursor.style.setProperty('--x', e.clientX + 'px');
            cursor.style.setProperty('--y', e.clientY + 'px');
            sessionStorage.setItem('demo-cursor', e.clientX + ',' + e.clientY);
          }, true);
          document.addEventListener('mousedown', (e) => {
            install();
            const ripple = document.createElement('div');
            ripple.className = 'demo-ripple';
            ripple.style.left = e.clientX + 'px';
            ripple.style.top = e.clientY + 'px';
            document.documentElement.appendChild(ripple);
            setTimeout(() => ripple.remove(), 700);
          }, true);
          if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', install); else install();
        })();
        """;

    private readonly Stopwatch _clock = Stopwatch.StartNew();
    private readonly List<(double Time, string File)> _frames = [];
    private readonly List<(string Id, double Time)> _steps = [];
    private readonly string _directory;
    private readonly IReadOnlyDictionary<string, double> _durations;
    private ICDPSession _cdp;
    private int _frameNumber;
    private double _cursorX = Width / 2.0;
    private double _cursorY = Height / 2.0;

    private Scene(string directory, IReadOnlyDictionary<string, double> durations)
    {
        _directory = directory;
        _durations = durations;
    }

    public IBrowserContext Context { get; private set; }

    public IPage Page { get; private set; }

    public string BaseUrl { get; private set; }

    public static async Task<Scene> StartAsync(IBrowser browser, string name, string baseUrl, string storageStatePath, string outputRoot, IReadOnlyDictionary<string, double> durations)
    {
        var directory = Path.Combine(outputRoot, name);

        if (Directory.Exists(directory))
        {
            Directory.Delete(directory, recursive: true);
        }

        Directory.CreateDirectory(Path.Combine(directory, "frames"));

        var scene = new Scene(directory, durations) { BaseUrl = baseUrl };
        scene.Context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = Width, Height = Height },
            DeviceScaleFactor = Scale,
            StorageStatePath = File.Exists(storageStatePath) ? storageStatePath : null,
            Locale = "en-US",
            ColorScheme = ColorScheme.Light,
        });
        await scene.Context.AddInitScriptAsync(OverlayScript);
        scene.Page = await scene.Context.NewPageAsync();
        scene.Page.SetDefaultTimeout(20_000);

        return scene;
    }

    /// <summary>
    /// Navigates before the capture starts, so the scene opens on a loaded page.
    /// </summary>
    public async Task OpenAsync(string path)
    {
        await Page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Page.Mouse.MoveAsync((float)_cursorX, (float)_cursorY);
        await Task.Delay(400);
    }

    /// <summary>
    /// Navigates during the capture, and keeps the screencast going on the new document.
    /// </summary>
    public async Task NavigateAsync(string path)
    {
        await Page.GotoAsync(BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await RestartScreencastAsync();
        await NudgeAsync();
    }

    // Clicks something that navigates, then keeps the screencast going on the new document.
    public async Task ClickAndWaitForNavigationAsync(ILocator locator)
    {
        await MoveToAsync(locator);
        await Task.Delay(180);
        await Page.RunAndWaitForNavigationAsync(() => Page.Mouse.ClickAsync((float)_cursorX, (float)_cursorY), new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await RestartScreencastAsync();
        await NudgeAsync();
        await Task.Delay(300);
    }

    /// <summary>
    /// Runs an action that navigates, such as a form submitted by a key or a menu item, and keeps capturing the new document.
    /// </summary>
    public async Task WaitForNavigationAsync(Func<Task> action)
    {
        await Page.RunAndWaitForNavigationAsync(action, new PageRunAndWaitForNavigationOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await RestartScreencastAsync();
        await NudgeAsync();
        await Task.Delay(300);
    }

    private async Task RestartScreencastAsync()
    {
        if (_cdp is null)
        {
            return;
        }

        try
        {
            await _cdp.SendAsync("Page.stopScreencast");
        }
        catch (PlaywrightException)
        {
        }

        await _cdp.SendAsync("Page.startScreencast", ScreencastOptions());
    }

    /// <summary>
    /// Captures another page of the context from now on, for instance a tab opened by a link.
    /// </summary>
    public async Task SwitchToAsync(IPage page)
    {
        if (_cdp is not null)
        {
            try
            {
                await _cdp.SendAsync("Page.stopScreencast");
            }
            catch (PlaywrightException)
            {
            }

            await _cdp.DetachAsync();
        }

        Page = page;
        await Page.BringToFrontAsync();
        await Page.SetViewportSizeAsync(Width, Height);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await StartCaptureCoreAsync();
        await NudgeAsync();
    }

    private static Dictionary<string, object> ScreencastOptions() => new()
    {
        ["format"] = "jpeg",
        ["quality"] = 92,
        ["maxWidth"] = (int)(Width * 1.125),
        ["maxHeight"] = (int)(Height * 1.125),
        ["everyNthFrame"] = 1,
    };

    public async Task StartCaptureAsync()
    {
        await StartCaptureCoreAsync();

        // A first frame, then a still moment before the narration starts.
        await NudgeAsync();
        await Task.Delay(600);
    }

    private async Task StartCaptureCoreAsync()
    {
        _cdp = await Context.NewCDPSessionAsync(Page);
        _cdp.Event("Page.screencastFrame").OnEvent += (_, args) =>
        {
            var data = args.Value.GetProperty("data").GetString();
            var sessionId = args.Value.GetProperty("sessionId").GetInt32();
            var file = Path.Combine(_directory, "frames", $"{_frameNumber++:D6}.jpg");
            File.WriteAllBytes(file, Convert.FromBase64String(data));

            lock (_frames)
            {
                _frames.Add((_clock.Elapsed.TotalSeconds, file));
            }

            _ = _cdp.SendAsync("Page.screencastFrameAck", new Dictionary<string, object> { ["sessionId"] = sessionId });
        };

        await _cdp.SendAsync("Page.startScreencast", ScreencastOptions());
    }

    /// <summary>
    /// Runs the actions of a narrated step, then holds until the step's narration is over.
    /// </summary>
    public async Task StepAsync(string id, Func<Task> actions = null)
    {
        var start = _clock.Elapsed.TotalSeconds;
        _steps.Add((id, start));
        Console.WriteLine($"  step {id} at {start:F1}s");

        if (actions is not null)
        {
            await actions();
        }

        var narration = _durations.TryGetValue(id, out var seconds) ? seconds : 2;
        var remaining = start + narration + 0.45 - _clock.Elapsed.TotalSeconds;

        if (remaining > 0)
        {
            await HoldAsync(remaining);
        }
    }

    // Keeps frames coming while nothing changes, so the screencast doesn't stall.
    public async Task HoldAsync(double seconds)
    {
        var until = _clock.Elapsed.TotalSeconds + seconds;

        while (_clock.Elapsed.TotalSeconds < until)
        {
            await Task.Delay((int)Math.Min(400, Math.Max(1, (until - _clock.Elapsed.TotalSeconds) * 1000)));
        }
    }

    public Task PauseAsync(int milliseconds) => Task.Delay(milliseconds);

    public async Task MoveToAsync(double x, double y, int steps = 25)
    {
        var fromX = _cursorX;
        var fromY = _cursorY;

        for (var i = 1; i <= steps; i++)
        {
            // Ease in and out.
            var t = i / (double)steps;
            var eased = t < .5 ? 2 * t * t : 1 - Math.Pow(-2 * t + 2, 2) / 2;
            _cursorX = fromX + (x - fromX) * eased;
            _cursorY = fromY + (y - fromY) * eased;
            await Page.Mouse.MoveAsync((float)_cursorX, (float)_cursorY);
            await Task.Delay(14);
        }
    }

    public async Task<(double X, double Y)> CenterOfAsync(ILocator locator)
    {
        await locator.ScrollIntoViewIfNeededAsync();
        var box = await locator.BoundingBoxAsync() ?? throw new InvalidOperationException($"No box for {locator}.");

        return (box.X + box.Width / 2, box.Y + box.Height / 2);
    }

    public async Task MoveToAsync(ILocator locator, double offsetX = 0, double offsetY = 0)
    {
        var (x, y) = await CenterOfAsync(locator);
        await MoveToAsync(x + offsetX, y + offsetY);
    }

    public async Task ClickAsync(ILocator locator, double offsetX = 0, double offsetY = 0, int pauseAfter = 500)
    {
        await MoveToAsync(locator, offsetX, offsetY);
        await Task.Delay(180);
        await Page.Mouse.ClickAsync((float)_cursorX, (float)_cursorY);
        await Task.Delay(pauseAfter);
    }

    public async Task ClickIfVisibleAsync(ILocator locator, int pauseAfter = 500)
    {
        if (await locator.IsVisibleAsync())
        {
            await ClickAsync(locator, pauseAfter: pauseAfter);
        }
    }

    public async Task DoubleClickAsync(ILocator locator, int pauseAfter = 600)
    {
        await MoveToAsync(locator);
        await Task.Delay(180);
        await Page.Mouse.DblClickAsync((float)_cursorX, (float)_cursorY);
        await Task.Delay(pauseAfter);
    }

    public async Task RightClickAsync(ILocator locator, int pauseAfter = 600)
    {
        await MoveToAsync(locator);
        await Task.Delay(180);
        await Page.Mouse.ClickAsync((float)_cursorX, (float)_cursorY, new MouseClickOptions { Button = MouseButton.Right });
        await Task.Delay(pauseAfter);
    }

    public async Task DragAsync(ILocator from, double toX, double toY)
    {
        await MoveToAsync(from);
        await Page.Mouse.DownAsync();
        await MoveToAsync(toX, toY, 40);
        await Page.Mouse.UpAsync();
        await Task.Delay(400);
    }

    public async Task TypeAsync(ILocator locator, string text, int delay = 55)
    {
        await ClickAsync(locator, pauseAfter: 200);
        await locator.PressSequentiallyAsync(text, new LocatorPressSequentiallyOptions { Delay = delay });
        await Task.Delay(300);
    }

    public async Task ScrollAsync(double deltaX, double deltaY, int steps = 12)
    {
        for (var i = 0; i < steps; i++)
        {
            await Page.Mouse.WheelAsync((float)(deltaX / steps), (float)(deltaY / steps));
            await Task.Delay(35);
        }

        await Task.Delay(300);
    }

    /// <summary>
    /// Draws a green ring around an element, with an optional caption, until <see cref="ClearHighlightsAsync"/>.
    /// </summary>
    public async Task HighlightAsync(ILocator locator, string caption = null, int padding = 6)
    {
        try
        {
            await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 3000 });
            await locator.ScrollIntoViewIfNeededAsync(new LocatorScrollIntoViewIfNeededOptions { Timeout = 3000 });
        }
        catch (Exception ex) when (ex is TimeoutException or PlaywrightException)
        {
            Console.WriteLine($"  (nothing to highlight: {locator})");
            return;
        }

        var box = await locator.BoundingBoxAsync();

        if (box is null)
        {
            return;
        }

        await Page.EvaluateAsync("""
            ({ x, y, w, h, p, caption }) => {
              const ring = document.createElement('div');
              ring.className = 'demo-highlight';
              Object.assign(ring.style, { left: (x - p) + 'px', top: (y - p) + 'px', width: (w + 2 * p) + 'px', height: (h + 2 * p) + 'px' });
              document.documentElement.appendChild(ring);
              if (caption) {
                const label = document.createElement('div');
                label.className = 'demo-caption';
                label.textContent = caption;
                const below = y + h + p + 10 + 40 < window.innerHeight;
                Object.assign(label.style, { left: Math.max(8, Math.min(x - p, window.innerWidth - 380)) + 'px', top: (below ? y + h + p + 10 : y - p - 44) + 'px' });
                label.dataset.demo = 'caption';
                document.documentElement.appendChild(label);
              }
            }
            """, new { x = (double)box.X, y = (double)box.Y, w = (double)box.Width, h = (double)box.Height, p = padding, caption });
    }

    public Task ClearHighlightsAsync()
        => Page.EvaluateAsync("() => document.querySelectorAll('.demo-highlight, .demo-caption').forEach(e => e.remove())");

    // Moves the cursor by a pixel, so the screencast sends a frame.
    public async Task NudgeAsync()
    {
        await Page.Mouse.MoveAsync((float)_cursorX + 1, (float)_cursorY);
        await Page.Mouse.MoveAsync((float)_cursorX, (float)_cursorY);
    }

    public async ValueTask DisposeAsync()
    {
        await HoldAsync(0.8);
        var end = _clock.Elapsed.TotalSeconds;

        if (_cdp is not null)
        {
            try
            {
                await _cdp.SendAsync("Page.stopScreencast");
            }
            catch (PlaywrightException)
            {
            }
        }

        await Task.Delay(300);

        List<(double Time, string File)> frames;

        lock (_frames)
        {
            frames = [.. _frames.OrderBy(frame => frame.Time)];
        }

        var origin = frames.Count > 0 ? frames[0].Time : 0;
        var timeline = new JsonObject
        {
            ["end"] = end - origin,
            ["frames"] = new JsonArray([.. frames.Select(frame => (JsonNode)new JsonObject { ["t"] = frame.Time - origin, ["file"] = Path.GetFileName(frame.File) })]),
            ["steps"] = new JsonArray([.. _steps.Select(step => (JsonNode)new JsonObject { ["id"] = step.Id, ["t"] = step.Time - origin })]),
        };

        await File.WriteAllTextAsync(Path.Combine(_directory, "timeline.json"), timeline.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        await Context.CloseAsync();
        Console.WriteLine($"  {frames.Count} frames, {end - origin:F1}s");
    }
}
