namespace Microsoft.Extensions.Options;

/// <summary>
/// Marks a type of options intended to be registered as a singleton and configured asynchronously
/// by an <see cref="IAsyncConfigureOptions{TOptions}"/> just after a tenant container is created.
/// </summary>
public interface IAsyncOptions
{
}
