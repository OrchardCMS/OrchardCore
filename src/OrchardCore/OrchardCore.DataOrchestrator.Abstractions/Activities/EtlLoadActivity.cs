namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Base class for ETL load activities that write data to a destination.
/// </summary>
public abstract class EtlLoadActivity : EtlActivity
{
    public override string Category => "Loads";
}
