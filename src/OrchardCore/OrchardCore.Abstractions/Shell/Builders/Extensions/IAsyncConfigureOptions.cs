using System.Threading.Tasks;

namespace Microsoft.Extensions.Options;

/// <summary>
/// Used to configure asynchronously the <typeparamref name="TOptions"/> type.
/// </summary>
public interface IAsyncConfigureOptions<TOptions> where TOptions : class, IAsyncOptions
{
    /// <summary>
    /// Invoked to configure asynchronously a <typeparamref name="TOptions"/> instance.
    /// </summary>
    Task ConfigureAsync(TOptions options);
}
