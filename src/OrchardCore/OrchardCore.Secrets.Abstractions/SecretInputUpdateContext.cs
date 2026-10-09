using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Secrets;

/// <summary>
/// The context used to apply a posted <see cref="SecretInputViewModel"/> to the values stored in the settings.
/// </summary>
public sealed class SecretInputUpdateContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SecretInputUpdateContext"/> class.
    /// </summary>
    /// <param name="services">The services of the current tenant, used to reach the Secrets module when it is enabled.</param>
    /// <param name="protector">The protector used for the value kept in the settings.</param>
    /// <param name="modelState">The model state that receives the validation errors.</param>
    /// <param name="htmlFieldPrefix">The full HTML field name of the <see cref="SecretInputViewModel"/>, used as the key of the validation errors.</param>
    public SecretInputUpdateContext(
        IServiceProvider services,
        IDataProtector protector,
        ModelStateDictionary modelState,
        string htmlFieldPrefix)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(protector);
        ArgumentNullException.ThrowIfNull(modelState);

        Services = services;
        Protector = protector;
        ModelState = modelState;
        HtmlFieldPrefix = htmlFieldPrefix;
    }

    /// <summary>
    /// Gets the services of the current tenant.
    /// </summary>
    public IServiceProvider Services { get; }

    /// <summary>
    /// Gets the protector used for the value kept in the settings.
    /// </summary>
    public IDataProtector Protector { get; }

    /// <summary>
    /// Gets the model state that receives the validation errors.
    /// </summary>
    public ModelStateDictionary ModelState { get; }

    /// <summary>
    /// Gets the full HTML field name of the <see cref="SecretInputViewModel"/>.
    /// </summary>
    public string HtmlFieldPrefix { get; }

    /// <summary>
    /// Gets or sets the protected value currently kept in the settings.
    /// </summary>
    public string ProtectedValue { get; set; }

    /// <summary>
    /// Gets or sets the name of the secret currently referenced by the settings.
    /// </summary>
    public string SecretName { get; set; }

    /// <summary>
    /// Gets or sets the description given to a secret created from the settings.
    /// </summary>
    public string Description { get; set; }
}
