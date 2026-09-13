using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

/// <summary>
/// Coordinates local index profiles with their provider resources.
/// </summary>
public interface IIndexProfileManagementService
{
    /// <summary>
    /// Validates and creates a profile and provider index, then schedules synchronization.
    /// A rejected provider creation removes the local profile; an exception propagates
    /// because the provider outcome may be uncertain. Scheduling does not mean indexing completed.
    /// </summary>
    Task<IndexProfileManagementResult> CreateAsync(IndexProfile profile);

    /// <summary>
    /// Removes the provider index before deleting the local profile. With force enabled,
    /// a missing provider or rejected provider deletion does not prevent local deletion.
    /// Provider exceptions still propagate.
    /// </summary>
    Task<IndexProfileManagementResult> DeleteAsync(IndexProfile profile, bool force = false);
}

/// <summary>
/// Describes the outcome of coordinating a profile and provider resource.
/// </summary>
public enum IndexProfileManagementResult
{
    /// <summary>The requested mutation completed; creation synchronization is only scheduled.</summary>
    Success,

    /// <summary>No index manager is registered for the profile's provider.</summary>
    ProviderUnavailable,

    /// <summary>The provider rejected the mutation.</summary>
    ProviderRejected,

    /// <summary>The local profile could not be removed, including creation compensation.</summary>
    LocalDeleteFailed,
}
