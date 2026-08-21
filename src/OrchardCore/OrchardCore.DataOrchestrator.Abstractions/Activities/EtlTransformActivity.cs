namespace OrchardCore.DataOrchestrator.Activities;

/// <summary>
/// Base class for ETL transform activities that modify data.
/// </summary>
public abstract class EtlTransformActivity : EtlActivity
{
    public override string Category => "Transforms";
}
