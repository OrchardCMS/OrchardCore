using System.Text.Json;
using Microsoft.Playwright;
using Recorder;

// Usage (VIDEO_ROOT is the workspace, VIDEO_BASE_URL the site, http://localhost:5010 by default):
//   Recorder setup                     logs in and saves the session in state.json
//   Recorder probe <path> <script>     runs a script on a page and prints the result (PROBE_SHOT=<png> saves a screenshot)
//   Recorder record <clip...>          records clips into clips/<clip>, "all" for every clip, timed by durations.json
var command = args.Length > 0 ? args[0] : throw new ArgumentException("Usage: Recorder setup | probe <path> <script> | record <clip...>");

using var playwright = await Playwright.CreateAsync();
await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

switch (command)
{
    case "setup":
        await SetupAsync(browser);
        break;

    case "probe":
        await ProbeAsync(browser, args[1], args[2]);
        break;

    case "record":
        var durations = JsonSerializer.Deserialize<Dictionary<string, double>>(File.ReadAllText(Path.Combine(Scenes.Root, "durations.json")));
        await Scenes.RecordAsync(browser, Path.Combine(Scenes.Root, "clips"), durations, args.Skip(1).ToArray());
        break;
}

static async Task SetupAsync(IBrowser browser)
{
    var context = await browser.NewContextAsync();
    var page = await context.NewPageAsync();

    // Waits for the site to start.
    for (var attempt = 0; ; attempt++)
    {
        try
        {
            var response = await page.GotoAsync(Scenes.BaseUrl + "/Login");

            if (response is { Ok: true })
            {
                break;
            }
        }
        catch (PlaywrightException) when (attempt < 60)
        {
        }

        await Task.Delay(2000);
    }

    await page.Locator("#LoginForm_UserName").FillAsync(Scenes.UserName);
    await page.Locator("#LoginForm_Password").FillAsync(Scenes.Password);
    await page.Locator("#LoginForm_Password").PressAsync("Enter");
    await page.WaitForURLAsync(url => !url.Contains("/Login", StringComparison.OrdinalIgnoreCase));

    await context.StorageStateAsync(new BrowserContextStorageStateOptions { Path = Scenes.StatePath });
    Console.WriteLine($"Logged in to {Scenes.BaseUrl}.");
    await context.CloseAsync();
}

static async Task ProbeAsync(IBrowser browser, string path, string script)
{
    var context = await browser.NewContextAsync(new BrowserNewContextOptions
    {
        StorageStatePath = Scenes.StatePath,
        ViewportSize = new ViewportSize { Width = Scene.Width, Height = Scene.Height },
        DeviceScaleFactor = Scene.Scale,
    });
    var page = await context.NewPageAsync();
    await page.GotoAsync(Scenes.BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
    await Task.Delay(800);
    Console.WriteLine((await page.EvaluateAsync<JsonElement>(script)).ToString());

    if (Environment.GetEnvironmentVariable("PROBE_SHOT") is { Length: > 0 } shot)
    {
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = shot });
    }

    await context.CloseAsync();
}
