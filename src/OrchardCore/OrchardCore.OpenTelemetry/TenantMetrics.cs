using System.Diagnostics.Metrics;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;

namespace OrchardCore.Telemetry;

/// <summary>
/// Observes the number of tenants of this instance of the application.
/// </summary>
internal sealed class TenantMetrics
{
    private static readonly TenantState[] s_states = Enum.GetValues<TenantState>();

    private readonly IShellHost _shellHost;

    public TenantMetrics(IShellHost shellHost, IMeterFactory meterFactory)
    {
        _shellHost = shellHost;

        var meter = meterFactory.Create(OrchardCoreTelemetryConstants.MeterName);

        // A gauge, as every instance of the application reports the same tenants: their counts don't add up.
        meter.CreateObservableGauge(
            OrchardCoreTelemetryConstants.Instruments.Tenants,
            ObserveTenants,
            unit: "{tenant}",
            description: "Number of tenants by state.");

        // An up-down counter, as the tenants loaded by the instances add up to the containers in memory.
        meter.CreateObservableUpDownCounter(
            OrchardCoreTelemetryConstants.Instruments.LoadedTenants,
            ObserveLoadedTenants,
            unit: "{tenant}",
            description: "Number of tenants whose service container is built and loaded in memory.");
    }

    private IEnumerable<Measurement<int>> ObserveTenants()
    {
        var counts = new int[s_states.Length];
        foreach (var settings in _shellHost.GetAllSettings())
        {
            counts[(int)settings.State]++;
        }

        var measurements = new Measurement<int>[s_states.Length];
        for (var i = 0; i < s_states.Length; i++)
        {
            measurements[i] = new(counts[i], new KeyValuePair<string, object>(OrchardCoreTelemetryConstants.Attributes.TenantState, s_states[i].ToString()));
        }

        return measurements;
    }

    private int ObserveLoadedTenants()
    {
        return _shellHost.ListShellContexts().Count(shell => !shell.Released && shell.ServiceProvider is not null);
    }
}
