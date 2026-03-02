namespace Microsoft.Extensions.Options;

/// <summary>
/// Represents a type used to configure asynchronously a <typeparamref name="TOptions"/> instance
/// during tenant container creation.
/// </summary>
/// <typeparam name="TOptions">The options type being configured.</typeparam>
/// <remarks>
/// <para>
/// This is the async equivalent of <see cref="IConfigureOptions{TOptions}"/>. Implementations are
/// registered using <c>services.ConfigureAsync&lt;TOptions, TConfigureOptions&gt;()</c> and their
/// <see cref="ConfigureAsync"/> method is invoked during tenant container creation.
/// </para>
/// <para>
/// <b>Important:</b> Async configuration only works with <see cref="IOptions{TOptions}"/>. When using
/// <see cref="IOptionsSnapshot{TOptions}"/> or <see cref="IOptionsMonitor{TOptions}"/>, async configurations
/// will be ignored because these interfaces create new options instances that bypass the async-configured instance.
/// </para>
/// <para>
/// Use this interface when async configuration requires dependency injection. For simple scenarios,
/// prefer the delegate-based <c>services.ConfigureAsync&lt;TOptions&gt;(async (sp, options) => ...)</c>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class MyOptionsConfiguration : IAsyncConfigureOptions&lt;MyOptions&gt;
/// {
///     private readonly ISiteService _siteService;
///     
///     public MyOptionsConfiguration(ISiteService siteService)
///         => _siteService = siteService;
///     
///     public async ValueTask ConfigureAsync(MyOptions options)
///     {
///         var settings = await _siteService.GetSettingsAsync&lt;MySettings&gt;();
///         options.SomeValue = settings.Value;
///     }
/// }
/// </code>
/// </example>
public interface IAsyncConfigureOptions<TOptions> where TOptions : class
{
    /// <summary>
    /// Configures the <typeparamref name="TOptions"/> instance asynchronously.
    /// </summary>
    /// <param name="options">The options instance to configure.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async operation.</returns>
    ValueTask ConfigureAsync(TOptions options);
}
