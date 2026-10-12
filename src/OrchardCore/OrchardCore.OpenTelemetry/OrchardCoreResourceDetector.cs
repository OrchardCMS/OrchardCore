using System.Reflection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Telemetry;

/// <summary>
/// Describes the application and this instance of it as resource attributes.
/// </summary>
internal sealed class OrchardCoreResourceDetector : IResourceDetector
{
    private readonly OrchardCoreOpenTelemetryOptions _options;
    private readonly IHostEnvironment _environment;

    public OrchardCoreResourceDetector(OrchardCoreOpenTelemetryOptions options, IHostEnvironment environment)
    {
        _options = options;
        _environment = environment;
    }

    public Resource Detect()
    {
        var serviceName = string.IsNullOrWhiteSpace(_options.ServiceName)
            ? _environment?.ApplicationName ?? Assembly.GetEntryAssembly()?.GetName().Name
            : _options.ServiceName;

        // Unique per running process, so that the metrics of instances sharing a machine don't get mixed up.
        var serviceInstanceId = string.IsNullOrWhiteSpace(_options.ServiceInstanceId)
            ? $"{System.Environment.MachineName}-{System.Environment.ProcessId}"
            : _options.ServiceInstanceId;

        var attributes = new Dictionary<string, object>
        {
            ["service.name"] = serviceName,
            ["service.instance.id"] = serviceInstanceId,
            ["host.name"] = System.Environment.MachineName,
            ["process.pid"] = System.Environment.ProcessId,
            ["process.runtime.name"] = ".NET",
            ["process.runtime.version"] = System.Environment.Version.ToString(),
        };

        AddIfNotNull(attributes, "service.version", GetVersion(Assembly.GetEntryAssembly()));
        AddIfNotNull(attributes, "deployment.environment.name", _environment?.EnvironmentName);
        AddIfNotNull(attributes, "orchardcore.version", GetVersion(typeof(ShellScope).Assembly));

        return new Resource(attributes);
    }

    private static void AddIfNotNull(Dictionary<string, object> attributes, string key, string value)
    {
        if (value is not null)
        {
            attributes[key] = value;
        }
    }

    private static string GetVersion(Assembly assembly)
        => assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
        ?? assembly?.GetName().Version?.ToString();
}
