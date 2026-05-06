namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Base class for ETL source activities that extract data.
/// </summary>
public abstract class EtlSourceActivity : EtlActivity
{
    public override string Category => "Sources";
}
