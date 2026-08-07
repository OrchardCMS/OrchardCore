namespace OrchardCore.Scripting.JavaScript;

/// <summary>
/// The settings of the JavaScript scripting engine, bound from the <c>OrchardCore_Scripting_JavaScript</c>
/// configuration section.
/// </summary>
public class JavaScriptEngineOptions
{
    /// <summary>
    /// Gets or sets how long a script may run before it fails with a <see cref="TimeoutException"/>.
    /// Defaults to 30 seconds.
    /// </summary>
    /// <remarks>
    /// A script is evaluated on the thread of its caller, which for a layer rule is the thread of the
    /// request being served, so without a timeout a script that never returns would hold that thread for
    /// as long as the process lives. <see cref="TimeSpan.Zero"/> and <see cref="TimeSpan.MaxValue"/> both
    /// mean that no timeout is applied at all.
    /// </remarks>
    public TimeSpan TimeoutInterval { get; set; } = TimeSpan.FromSeconds(30);
}
