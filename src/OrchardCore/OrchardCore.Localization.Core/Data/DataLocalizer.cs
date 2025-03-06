using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace OrchardCore.Localization.Data;

public class DataLocalizer : IDataLocalizer
{
    private readonly DataResourceManager _dataResourceManager;
    private readonly bool _fallBackToParentCulture;
    private readonly ILogger _logger;

    public DataLocalizer(DataResourceManager dataResourceManager, bool fallBackToParentCulture, ILogger logger)
    {
        _dataResourceManager = dataResourceManager;
        _fallBackToParentCulture = fallBackToParentCulture;
        _logger = logger;
    }

    /// <inheritdoc/>
    public DataLocalizedString this[string name, string context]
    {
        get
        {
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
            ArgumentNullException.ThrowIfNullOrEmpty(context, nameof(context));

            var translation = GetTranslation(name, context, CultureInfo.CurrentUICulture);

            return new DataLocalizedString(context, name, translation ?? name, translation == null);
        }
    }

    /// <inheritdoc/>
    public DataLocalizedString this[string name, string context, params object[] arguments]
    {
        get
        {
            var translation = this[name, context];
            var localizedString = new DataLocalizedString(name, context, translation, translation.ResourceNotFound);
            var formatted = string.Format(localizedString.Value, arguments);

            return new DataLocalizedString(context, name, formatted, translation.ResourceNotFound);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<DataLocalizedString> GetAllStrings(string context, bool includeParentCultures)
    {
        var culture = CultureInfo.CurrentUICulture;

        var translations = _dataResourceManager.GetResources(culture, includeParentCultures);

        foreach (var translation in translations)
        {
            yield return new DataLocalizedString(context, translation.Key, translation.Value);
        }
    }

    private string GetTranslation(string name, string context, CultureInfo culture)
    {
        string translation = null;
        try
        {
            if (_fallBackToParentCulture)
            {
                do
                {
                    translation = _dataResourceManager.GetString(name, context, culture);

                    if (translation != null)
                    {
                        break;
                    }

                    culture = culture.Parent;
                }
                while (culture != CultureInfo.InvariantCulture);
            }
            else
            {
                translation = _dataResourceManager.GetString(name, context);
            }
        }
        catch (PluralFormNotFoundException ex)
        {
            _logger.LogWarning(ex, "There is no pluralization form for the resource string '{Name}' with the culture '{Culture}'.", name, culture.Name);
        }

        return translation;
    }
}
