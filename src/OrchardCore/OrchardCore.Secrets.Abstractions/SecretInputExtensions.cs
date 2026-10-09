using System.Security.Cryptography;
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
    /// Validates the posted credential and computes the values to store in the settings. When the Secrets feature is
    /// enabled, this also creates the secret requested with <see cref="SecretInputSource.NewSecret"/>.
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
        var source = secretManager is null ? SecretInputSource.Value : model.Source;

        switch (source)
        {
            case SecretInputSource.Secret:
                if (string.IsNullOrWhiteSpace(model.SecretName))
                {
                    AddError(context, nameof(SecretInputViewModel.SecretName), S["Select a secret."]);

                    return Failed();
                }

                if (!await SecretExistsAsync(secretManager, model.SecretName))
                {
                    AddError(context, nameof(SecretInputViewModel.SecretName), S["The secret '{0}' does not exist.", model.SecretName]);

                    return Failed();
                }

                return Succeeded(null, model.SecretName.Trim());

            case SecretInputSource.NewSecret:
                return await CreateSecretAsync(model, context, secretManager, S);

            default:
                if (!string.IsNullOrWhiteSpace(model.Value))
                {
                    return Succeeded(context.Protector.Protect(model.Value), null);
                }

                if (secretManager is null)
                {
                    return Succeeded(context.ProtectedValue, context.SecretName);
                }

                // Switching back from a secret to a value requires a value when none is kept in the settings.
                if (!string.IsNullOrWhiteSpace(context.SecretName) && string.IsNullOrWhiteSpace(context.ProtectedValue))
                {
                    AddError(context, nameof(SecretInputViewModel.Value), S["Enter a value, or select a secret."]);

                    return Failed();
                }

                return Succeeded(context.ProtectedValue, null);
        }
    }

    private static async Task<SecretInputResult> CreateSecretAsync(
        SecretInputViewModel model,
        SecretInputUpdateContext context,
        ISecretManager secretManager,
        IStringLocalizer S)
    {
        var secretName = model.NewSecretName?.Trim();

        if (string.IsNullOrEmpty(secretName))
        {
            AddError(context, nameof(SecretInputViewModel.NewSecretName), S["Enter the name of the new secret."]);

            return Failed();
        }

        if (await SecretExistsAsync(secretManager, secretName))
        {
            AddError(context, nameof(SecretInputViewModel.NewSecretName), S["A secret named '{0}' already exists. Select it instead, or enter another name.", secretName]);

            return Failed();
        }

        var value = model.Value;

        if (string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(context.ProtectedValue))
        {
            try
            {
                // Move the value kept in the settings.
                value = context.Protector.Unprotect(context.ProtectedValue);
            }
            catch (CryptographicException)
            {
                AddError(context, nameof(SecretInputViewModel.Value), S["The current value could not be decrypted. Enter the value to store as a secret."]);

                return Failed();
            }
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            AddError(context, nameof(SecretInputViewModel.Value), S["Enter the value to store as a secret."]);

            return Failed();
        }

        await secretManager.SaveSecretAsync(secretName, new TextSecret { Text = value }, new SecretSaveOptions
        {
            Description = context.Description,
        });

        return Succeeded(null, secretName);
    }

    private static async Task<bool> SecretExistsAsync(ISecretManager secretManager, string secretName)
    {
        var infos = await secretManager.GetSecretInfosAsync();

        return infos.Any(info => string.Equals(info.Name, secretName.Trim(), StringComparison.OrdinalIgnoreCase));
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
