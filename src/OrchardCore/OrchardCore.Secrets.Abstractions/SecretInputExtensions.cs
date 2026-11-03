using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Secrets;

/// <summary>
/// Applies a posted <see cref="SecretInputViewModel"/> to the values stored in the settings.
/// </summary>
public static class SecretInputExtensions
{
    /// <summary>
    /// Validates the posted credential and computes the values to store in the settings.
    /// </summary>
    /// <param name="model">The posted credential.</param>
    /// <param name="context">The values currently stored and the services used to update them.</param>
    /// <returns>The values to store. When the result did not succeed, errors were added to the model state.</returns>
    public static async Task<SecretInputResult> UpdateAsync(this SecretInputViewModel model, SecretInputUpdateContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        model ??= new SecretInputViewModel();

        var secretManager = context.Services.GetService<ISecretManager>();
        var S = context.Services.GetRequiredService<IStringLocalizer<SecretInputViewModel>>();

        // Without the Secrets feature only a value can be entered, and a secret reference made while it was enabled is kept.
        if (secretManager is null)
        {
            return string.IsNullOrWhiteSpace(model.Value)
                ? Succeeded(context.ProtectedValue, context.SecretName)
                : Succeeded(context.Protector.Protect(model.Value), null);
        }

        if (model.Source == SecretInputSource.Secret)
        {
            if (string.IsNullOrWhiteSpace(model.SecretName))
            {
                AddError(context, nameof(SecretInputViewModel.SecretName), S["Select a secret."]);

                return Failed();
            }

            var secretName = model.SecretName.Trim();
            var infos = await secretManager.GetSecretInfosAsync();

            if (!infos.Any(info => string.Equals(info.Name, secretName, StringComparison.OrdinalIgnoreCase)))
            {
                AddError(context, nameof(SecretInputViewModel.SecretName), S["The secret '{0}' does not exist.", secretName]);

                return Failed();
            }

            // The value kept in the settings is no longer used once a secret is referenced.
            return Succeeded(null, secretName);
        }

        if (!string.IsNullOrWhiteSpace(model.Value))
        {
            return Succeeded(context.Protector.Protect(model.Value), null);
        }

        // Switching back from a secret to a value requires a value when none is kept in the settings.
        if (!string.IsNullOrWhiteSpace(context.SecretName) && string.IsNullOrWhiteSpace(context.ProtectedValue))
        {
            AddError(context, nameof(SecretInputViewModel.Value), S["Enter a value, or select a secret."]);

            return Failed();
        }

        return Succeeded(context.ProtectedValue, null);
    }

    private static void AddError(SecretInputUpdateContext context, string propertyName, string message)
    {
        var key = string.IsNullOrEmpty(context.HtmlFieldPrefix)
            ? propertyName
            : context.HtmlFieldPrefix + "." + propertyName;

        context.ModelState.AddModelError(key, message);
    }

    private static SecretInputResult Succeeded(string protectedValue, string secretName)
        => new()
        {
            Succeeded = true,
            ProtectedValue = protectedValue,
            SecretName = secretName,
        };

    private static SecretInputResult Failed()
        => new()
        {
            Succeeded = false,
        };
}
