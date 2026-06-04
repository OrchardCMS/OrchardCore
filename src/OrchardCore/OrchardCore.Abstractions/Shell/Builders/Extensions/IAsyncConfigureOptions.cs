namespace Microsoft.Extensions.Options;

/// <summary>
/// Used to configure asynchronously a type of options just after a tenant container is created.
/// </summary>
[Obsolete("This interface is no longer supported and will be removed in a future version. " +
    "Use IOptions instead and configure it synchronously.")]
public interface IAsyncConfigureOptions<TOptions> where TOptions : class, IAsyncOptions
{
    /// <summary>
    /// Configures asynchronously an options instance.
    /// </summary>
    ValueTask ConfigureAsync(TOptions options);
}
