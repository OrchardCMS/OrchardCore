using Microsoft.Playwright;

namespace Recorder;

/// <summary>
/// The clips of the video. Replace the example clip with your own: one method per "clip" of storyboard.json, with
/// one <see cref="Scene.StepAsync"/> per narrated step, in the order of the storyboard.
/// </summary>
public static class Scenes
{
    public static string BaseUrl { get; } = Environment.GetEnvironmentVariable("VIDEO_BASE_URL") ?? "http://localhost:5010";

    public static string UserName { get; } = Environment.GetEnvironmentVariable("VIDEO_USER_NAME") ?? "admin";

    // A throwaway local site: see references/recording.md.
    public static string Password { get; } = Environment.GetEnvironmentVariable("VIDEO_PASSWORD") ?? "Password1!";

    public static string Root { get; } = Environment.GetEnvironmentVariable("VIDEO_ROOT") ?? throw new InvalidOperationException("Set VIDEO_ROOT to the workspace.");

    public static string StatePath => Path.Combine(Root, "state.json");

    // The clips, in the order they are recorded: each one starts from the state the previous one left.
    private static readonly (string Name, Func<IBrowser, string, IReadOnlyDictionary<string, double>, Task> Record)[] s_clips =
    [
        ("02-features", FeaturesAsync),
    ];

    public static async Task RecordAsync(IBrowser browser, string outputRoot, IReadOnlyDictionary<string, double> durations, string[] names)
    {
        var selected = names is ["all"] ? s_clips : s_clips.Where(clip => names.Contains(clip.Name)).ToArray();

        foreach (var (name, record) in selected)
        {
            Console.WriteLine($"Recording {name}");
            await record(browser, Path.Combine(outputRoot, name), durations);
        }
    }

    private static Task<Scene> NewSceneAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
        => Scene.StartAsync(browser, Path.GetFileName(directory), BaseUrl, StatePath, Path.GetDirectoryName(directory), durations);

    // An example clip: searches the features, and points at one of them.
    private static async Task FeaturesAsync(IBrowser browser, string directory, IReadOnlyDictionary<string, double> durations)
    {
        await using var scene = await NewSceneAsync(browser, directory, durations);

        // Everything before StartCaptureAsync is not recorded: open the page, and get it ready.
        await scene.OpenAsync("/Admin/Features");
        await scene.StartCaptureAsync();

        ILocator Row(string feature)
            => scene.Page.Locator(".list-group-item").Filter(new() { Has = scene.Page.Locator($"#btn-enable-{feature}, #btn-disable-{feature}") }).First;

        await scene.StepAsync("features-1", async () =>
        {
            await scene.TypeAsync(scene.Page.Locator("#search-box"), "secret", delay: 90);
        });

        await scene.StepAsync("features-2", async () =>
        {
            await scene.MoveToAsync(Row("OrchardCore_Secrets"), -200);
            await scene.HighlightAsync(Row("OrchardCore_Secrets"), "The core feature");
        });

        await scene.ClearHighlightsAsync();
    }
}
