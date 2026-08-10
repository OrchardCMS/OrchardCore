using Jint;
using Microsoft.Extensions.Options;
using JintOptions = Jint.Options;

namespace OrchardCore.Scripting.JavaScript;

/// <summary>
/// Applies the execution constraints of <see cref="JavaScriptEngineOptions"/> to the options every engine
/// of the tenant is built from.
/// </summary>
/// <remarks>
/// Registered by <see cref="ServiceCollectionExtensions.AddJavaScriptEngine"/>, and therefore before any
/// configuration of <see cref="JintOptions"/> an application adds, so that an application can still change
/// or remove the constraints configured here.
/// </remarks>
internal sealed class JintOptionsConfiguration : IConfigureOptions<JintOptions>
{
    private readonly JavaScriptEngineOptions _javaScriptEngineOptions;

    public JintOptionsConfiguration(IOptions<JavaScriptEngineOptions> javaScriptEngineOptions)
    {
        _javaScriptEngineOptions = javaScriptEngineOptions.Value;
    }

    public void Configure(JintOptions options)
    {
        // A non-positive interval, and TimeSpan.MaxValue, register no constraint, which is how the timeout
        // is turned off from configuration.
        options.TimeoutInterval(_javaScriptEngineOptions.TimeoutInterval);
    }
}
