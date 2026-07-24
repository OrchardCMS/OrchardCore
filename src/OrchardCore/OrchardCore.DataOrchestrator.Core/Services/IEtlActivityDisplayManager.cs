using OrchardCore.DisplayManagement;
using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.DataOrchestrator.Services;

/// <summary>
/// Manages the display of ETL activities using display drivers.
/// </summary>
public interface IEtlActivityDisplayManager : IDisplayManager<IEtlActivity>;
