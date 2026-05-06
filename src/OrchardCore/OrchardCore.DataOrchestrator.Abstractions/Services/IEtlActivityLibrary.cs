using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Provides access to the registered ETL activities.
/// </summary>
public interface IEtlActivityLibrary
{
    /// <summary>
    /// Returns instances of all registered activities.
    /// </summary>
    IEnumerable<IEtlActivity> ListActivities();

    /// <summary>
    /// Returns the distinct categories of all registered activities.
    /// </summary>
    IEnumerable<string> ListCategories();

    /// <summary>
    /// Returns the activity instance with the specified name.
    /// </summary>
    IEtlActivity GetActivityByName(string name);

    /// <summary>
    /// Creates a new instance of the activity with the specified name.
    /// </summary>
    IEtlActivity InstantiateActivity(string name);
}
