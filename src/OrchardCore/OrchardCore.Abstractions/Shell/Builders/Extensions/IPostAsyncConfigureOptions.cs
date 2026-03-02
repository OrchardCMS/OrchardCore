namespace Microsoft.Extensions.Options;

/// <summary>
/// Represents a type used to perform post-configuration of a <typeparamref name="TOptions"/> instance
/// asynchronously, after all <see cref="IAsyncConfigureOptions{TOptions}"/> have run.
/// </summary>
/// <typeparam name="TOptions">The options type being configured.</typeparam>
/// <remarks>
/// <para>
/// This is the async equivalent of <see cref="IPostConfigureOptions{TOptions}"/>. Implementations are
/// registered using <c>services.PostConfigureAsync&lt;TOptions, TPostConfigureOptions&gt;()</c> and their
/// <see cref="PostConfigureAsync"/> method is invoked during tenant container creation, after all
/// <see cref="IAsyncConfigureOptions{TOptions}"/> implementations have run.
/// </para>
/// <para>
/// <b>Important:</b> Async configuration only works with <see cref="IOptions{TOptions}"/>. When using
/// <see cref="IOptionsSnapshot{TOptions}"/> or <see cref="IOptionsMonitor{TOptions}"/>, async configurations
/// will be ignored because these interfaces create new options instances that bypass the async-configured instance.
/// </para>
/// <para>
/// The execution order for options configuration is:
/// </para>
/// <list type="number">
///   <item><description>All <see cref="IConfigureOptions{TOptions}"/> (sync, in registration order)</description></item>
///   <item><description>All <see cref="IPostConfigureOptions{TOptions}"/> (sync, in registration order)</description></item>
///   <item><description>All <see cref="IAsyncConfigureOptions{TOptions}"/> (async, in registration order)</description></item>
///   <item><description>All <see cref="IPostAsyncConfigureOptions{TOptions}"/> (async, in registration order)</description></item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// public class MyOptionsPostConfiguration : IPostAsyncConfigureOptions&lt;MyOptions&gt;
/// {
///     private readonly IValidator _validator;
///     
///     public MyOptionsPostConfiguration(IValidator validator)
///         => _validator = validator;
///     
///     public async ValueTask PostConfigureAsync(MyOptions options)
///     {
///         await _validator.ValidateAsync(options);
///     }
/// }
/// </code>
/// </example>
public interface IPostAsyncConfigureOptions<TOptions> where TOptions : class
{
    /// <summary>
    /// Post-configures the <typeparamref name="TOptions"/> instance asynchronously.
    /// </summary>
    /// <param name="options">The options instance to post-configure.</param>
    /// <returns>A <see cref="ValueTask"/> representing the async operation.</returns>
    ValueTask PostConfigureAsync(TOptions options);
}
