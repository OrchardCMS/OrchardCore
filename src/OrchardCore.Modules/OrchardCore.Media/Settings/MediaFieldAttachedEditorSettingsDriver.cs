using System.Buffers;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Media.Fields;
using OrchardCore.Media.ViewModels;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Media.Settings;

public sealed class MediaFieldAttachedEditorSettingsDriver : ContentPartFieldDefinitionDisplayDriver<MediaField>
{
    private static readonly HashSet<string> _reservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "CON", "PRN", "AUX", "NUL",
        "COM0", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9",
        "LPT0", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9",
    };

    private static readonly SearchValues<char> _invalidFolderNameSearchValues = SearchValues.Create(
    [
        '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\0',
        '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008',
        '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
        '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
        '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
    ]);

    private static readonly char[] _invalidFolderNameChars =
    [
        '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\0',
        '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007', '\u0008',
        '\u0009', '\u000A', '\u000B', '\u000C', '\u000D', '\u000E', '\u000F',
        '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017',
        '\u0018', '\u0019', '\u001A', '\u001B', '\u001C', '\u001D', '\u001E', '\u001F',
    ];

    private readonly IMediaFileStore _mediaFileStore;
    private readonly MediaOptions _mediaOptions;

    internal readonly IStringLocalizer S;

    public MediaFieldAttachedEditorSettingsDriver(
        IMediaFileStore mediaFileStore,
        IOptions<MediaOptions> mediaOptions,
        IStringLocalizer<MediaFieldAttachedEditorSettingsDriver> stringLocalizer)
    {
        _mediaFileStore = mediaFileStore;
        _mediaOptions = mediaOptions.Value;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ContentPartFieldDefinition partFieldDefinition, BuildEditorContext context)
    {
        return Initialize<MediaFieldAttachedEditorSettingsViewModel>("MediaFieldAttachedEditorSettings_Edit", model =>
        {
            var settings = partFieldDefinition.GetSettings<MediaFieldAttachedEditorSettings>();
            model.Folder = settings.Folder;
        }).Location("Editor");
    }

    public override async Task<IDisplayResult> UpdateAsync(ContentPartFieldDefinition partFieldDefinition, UpdatePartFieldEditorContext context)
    {
        if (partFieldDefinition.Editor() == "Attached")
        {
            var model = new MediaFieldAttachedEditorSettingsViewModel();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            var folder = SanitizeFolderName(model.Folder);

            if (!string.IsNullOrEmpty(folder))
            {
                if (!IsValidFolderName(folder))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Folder), S["The folder name contains invalid characters or is a reserved name."]);
                }
                else if (IsReservedMediaFolder(folder))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.Folder), S["The folder name '{0}' is reserved and cannot be used.", folder]);
                }
            }

            if (context.Updater.ModelState.IsValid)
            {
                var settings = new MediaFieldAttachedEditorSettings
                {
                    Folder = folder,
                };

                context.Builder.WithSettings(settings);

                // Create the folder immediately so the Secure Media feature can
                // generate the per-folder permission right away. The TryCreateDirectoryAsync
                // call also fires the MediaCreatedDirectory event which signals
                // SecureMediaPermissions to rebuild its cached permission list.
                if (!string.IsNullOrEmpty(folder))
                {
                    await _mediaFileStore.TryCreateDirectoryAsync(folder);
                }
            }
        }

        return Edit(partFieldDefinition, context);
    }

    internal static string SanitizeFolderName(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder))
        {
            return null;
        }

        // Trim whitespace and path separators.
        folder = folder.Trim().Trim('/', '\\', ' ');

        // Remove any invalid characters.
        foreach (var c in _invalidFolderNameChars)
        {
            folder = folder.Replace(c.ToString(), string.Empty);
        }

        // Trim trailing dots and spaces (Windows restriction).
        folder = folder.TrimEnd('.', ' ');

        return string.IsNullOrEmpty(folder) ? null : folder;
    }

    internal static bool IsValidFolderName(string folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return false;
        }

        // Disallow path traversal.
        if (folder.Contains("..") || folder.Contains('/') || folder.Contains('\\'))
        {
            return false;
        }

        // Check for any remaining invalid characters.
        if (folder.AsSpan().IndexOfAny(_invalidFolderNameSearchValues) >= 0)
        {
            return false;
        }

        // Check for reserved device names.
        var nameWithoutExtension = folder.Contains('.') ? folder[..folder.IndexOf('.')] : folder;
        if (_reservedNames.Contains(nameWithoutExtension))
        {
            return false;
        }

        return true;
    }

    private bool IsReservedMediaFolder(string folder)
    {
        return string.Equals(folder, _mediaOptions.AssetsUsersFolder, StringComparison.OrdinalIgnoreCase)
            || string.Equals(folder, "mediafields", StringComparison.OrdinalIgnoreCase);
    }
}
