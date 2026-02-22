using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings.ViewModels;

namespace OrchardCore.Settings.Drivers;

/// <summary>
/// Base display driver for configurable site settings that provides automatic metadata handling,
/// read-only mode support, and consistent authorization.
/// </summary>
/// <typeparam name="TSettings">The type of settings to display.</typeparam>
/// <typeparam name="TViewModel">The type of view model used for editing.</typeparam>
public abstract class ConfigurableSiteSettingsDisplayDriver<TSettings, TViewModel> : SiteDisplayDriver<TSettings>
    where TSettings : class, IConfigurableSettings, new()
    where TViewModel : ConfigurableSettingsViewModel<TSettings>, new()
{
    private readonly IConfigurableSettingsService<TSettings> _settingsService;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurableSiteSettingsDisplayDriver{TSettings, TViewModel}"/> class.
    /// </summary>
    protected ConfigurableSiteSettingsDisplayDriver(
        IConfigurableSettingsService<TSettings> settingsService,
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService)
    {
        _settingsService = settingsService;
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
    }

    /// <summary>
    /// Gets the shape type for the edit view.
    /// </summary>
    protected abstract string EditShapeType { get; }

    /// <summary>
    /// Gets the shape type for the read-only view.
    /// Returns <c>null</c> to use the same shape as edit with <see cref="ConfigurableSettingsViewModel{TSettings}.IsReadOnly"/> set to <c>true</c>.
    /// </summary>
    protected virtual string ReadOnlyShapeType => null;

    /// <summary>
    /// Gets the permission required to view/edit these settings.
    /// </summary>
    protected abstract Permission RequiredPermission { get; }

    /// <summary>
    /// Gets the display location for the shape.
    /// Defaults to "Content:2".
    /// </summary>
    protected virtual string DisplayLocation => "Content:2";

    /// <inheritdoc/>
    public override async Task<IDisplayResult> EditAsync(ISite site, TSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, RequiredPermission))
        {
            return null;
        }

        context.AddTenantReloadWarningWrapper();

        var metadata = await _settingsService.GetMetadataAsync();
        var effectiveSettings = await _settingsService.GetEffectiveSettingsAsync();
        var isReadOnly = metadata.DisableUIConfiguration;

        var shapeType = isReadOnly && !string.IsNullOrEmpty(ReadOnlyShapeType)
            ? ReadOnlyShapeType
            : EditShapeType;

        return Initialize<TViewModel>(shapeType, model =>
        {
            model.Metadata = metadata;
            model.IsReadOnly = isReadOnly;

            // Pass the database settings for form binding
            // and effective settings for display of overridden values
            PopulateViewModel(model, settings, effectiveSettings);
        })
        .Location(DisplayLocation)
        .OnGroup(SettingsGroupId);
    }

    /// <inheritdoc/>
    public override async Task<IDisplayResult> UpdateAsync(ISite site, TSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, RequiredPermission))
        {
            return null;
        }

        var metadata = await _settingsService.GetMetadataAsync();

        // If UI configuration is disabled, don't allow updates
        if (metadata.DisableUIConfiguration)
        {
            return await EditAsync(site, settings, context);
        }

        var model = new TViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        // Validate the model
        if (!await ValidateModelAsync(model, context, metadata))
        {
            return await EditAsync(site, settings, context);
        }

        // Update settings from the view model
        UpdateSettings(settings, model, metadata);

        // Request shell release to apply changes
        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }

    /// <summary>
    /// Populates the view model with values from the database settings and effective settings.
    /// </summary>
    /// <param name="model">The view model to populate.</param>
    /// <param name="databaseSettings">The settings as stored in the database.</param>
    /// <param name="effectiveSettings">The effective settings after merging.</param>
    /// <remarks>
    /// Override this method to map settings properties to view model properties.
    /// Use <paramref name="databaseSettings"/> for form field values (what the user sees in inputs).
    /// Use <paramref name="effectiveSettings"/> for displaying the actual effective value when overridden.
    /// </remarks>
    protected abstract void PopulateViewModel(TViewModel model, TSettings databaseSettings, TSettings effectiveSettings);

    /// <summary>
    /// Updates the settings from the view model.
    /// </summary>
    /// <param name="settings">The settings to update.</param>
    /// <param name="model">The view model with updated values.</param>
    /// <param name="metadata">The configuration metadata.</param>
    /// <remarks>
    /// Override this method to map view model properties back to settings properties.
    /// Consider checking <see cref="PropertyConfigurationMetadata.CanConfigureViaUI"/> before updating
    /// properties that may be file-only.
    /// </remarks>
    protected abstract void UpdateSettings(TSettings settings, TViewModel model, SettingsConfigurationMetadata metadata);

    /// <summary>
    /// Validates the view model before updating settings.
    /// </summary>
    /// <param name="model">The view model to validate.</param>
    /// <param name="context">The update context.</param>
    /// <param name="metadata">The configuration metadata.</param>
    /// <returns><c>true</c> if the model is valid; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// Override this method to add custom validation logic.
    /// Use <paramref name="context"/>.Updater.ModelState to add validation errors.
    /// </remarks>
    protected virtual Task<bool> ValidateModelAsync(TViewModel model, UpdateEditorContext context, SettingsConfigurationMetadata metadata)
        => Task.FromResult(context.Updater.ModelState.IsValid);

    /// <summary>
    /// Checks if a property should be updated based on its configuration.
    /// </summary>
    /// <param name="propertyName">The property name.</param>
    /// <param name="metadata">The configuration metadata.</param>
    /// <returns><c>true</c> if the property can be updated; otherwise, <c>false</c>.</returns>
    protected bool ShouldUpdateProperty(string propertyName, SettingsConfigurationMetadata metadata)
    {
        var propertyMetadata = metadata.GetPropertyMetadata(propertyName);
        return propertyMetadata?.CanConfigureViaUI ?? true;
    }
}
