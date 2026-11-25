using System.Text.Json.Nodes;
using Microsoft.Extensions.Localization;
using OrchardCore.Recipes.Models;
using OrchardCore.Recipes.Services;
using OrchardCore.Themes.TheAdmin.Deployment;
using OrchardCore.Themes.TheAdmin.Models;
using OrchardCore.Themes.TheAdmin.Services;

namespace OrchardCore.Themes.TheAdmin.Recipes;

/// <summary>
/// The 'AdminSchemes' recipe step. A scheme whose 'ItemId' matches a stored one replaces it, any other is added (with a
/// new id when it has none). The optional 'DefaultSchemeId' makes a scheme the site's default scheme.
/// </summary>
public sealed class AdminSchemesStep : NamedRecipeStepHandler
{
    private readonly AdminSchemeStore _store;

    internal readonly IStringLocalizer S;

    public AdminSchemesStep(AdminSchemeStore store, IStringLocalizer<AdminSchemesStep> stringLocalizer)
        : base(AdminSchemesDeploymentStep.StepName)
    {
        _store = store;
        S = stringLocalizer;
    }

    protected override async Task HandleAsync(RecipeExecutionContext context)
    {
        if (context.Step[AdminSchemesDeploymentStep.EntriesName] is JsonArray entries)
        {
            var position = await _store.NextPositionAsync();

            foreach (var entry in entries.OfType<JsonObject>())
            {
                position = await ImportAsync(context, entry, position);
            }
        }

        var defaultId = context.Step[AdminSchemesDeploymentStep.DefaultSchemeIdName]?.GetValue<string>();

        if (!string.IsNullOrEmpty(defaultId) && !await _store.SetDefaultAsync(defaultId))
        {
            context.Errors.Add(S["There is no scheme '{0}' to make the default scheme.", defaultId]);
        }
    }

    // Returns the position of the next entry that names none.
    private async Task<int> ImportAsync(RecipeExecutionContext context, JsonObject entry, int position)
    {
        // The properties whose JSON value is not of the expected type (a string, a number, a boolean): the entry is
        // reported rather than the recipe stopped.
        var invalid = new List<string>();

        var id = ReadString(entry, AdminSchemesDeploymentStep.ItemIdName, invalid);

        if (string.IsNullOrEmpty(id))
        {
            id = AdminSchemeStore.NewId();
        }

        var scheme = new AdminScheme
        {
            Id = id,
            Name = ReadString(entry, nameof(AdminScheme.Name), invalid)?.Trim(),
            Primary = ReadString(entry, nameof(AdminScheme.Primary), invalid),
            Deep = ReadString(entry, nameof(AdminScheme.Deep), invalid),
            Ink = ReadString(entry, nameof(AdminScheme.Ink), invalid),
            Accent = ReadString(entry, nameof(AdminScheme.Accent), invalid),
            LightMenu = Read<bool>(entry, nameof(AdminScheme.LightMenu), invalid) ?? false,
            Position = Read<int>(entry, nameof(AdminScheme.Position), invalid) ?? position++,
            Hue = Read<double>(entry, nameof(AdminScheme.Hue), invalid),
            Saturation = Read<double>(entry, nameof(AdminScheme.Saturation), invalid),
            OnPrimary = ReadString(entry, nameof(AdminScheme.OnPrimary), invalid),
            Page = ReadString(entry, nameof(AdminScheme.Page), invalid),
            Toolbar = ReadString(entry, nameof(AdminScheme.Toolbar), invalid),
            Tint = ReadString(entry, nameof(AdminScheme.Tint), invalid),
            TintDark = ReadString(entry, nameof(AdminScheme.TintDark), invalid),
            FontFamily = ReadString(entry, nameof(AdminScheme.FontFamily), invalid),
            HeadingFontFamily = ReadString(entry, nameof(AdminScheme.HeadingFontFamily), invalid),
            FontScale = Read<double>(entry, nameof(AdminScheme.FontScale), invalid),
            RadiusScale = Read<double>(entry, nameof(AdminScheme.RadiusScale), invalid),
            Density = Read<double>(entry, nameof(AdminScheme.Density), invalid),
        };

        if (invalid.Count > 0)
        {
            context.Errors.Add(S["The scheme '{0}' has values of the wrong type: {1}. ItemId, Name, the colors and the fonts are strings, LightMenu is a boolean, Position is a whole number, and the other settings are numbers.", scheme.Id, string.Join(", ", invalid)]);

            return position;
        }

        if (!AdminSchemeStore.IsValidId(scheme.Id))
        {
            context.Errors.Add(S["The scheme id '{0}' is not valid: it must start with a lowercase letter and hold only lowercase letters, digits and dashes.", scheme.Id]);

            return position;
        }

        if (string.IsNullOrEmpty(scheme.Name) || !AdminSchemeStore.HasValidColors(scheme))
        {
            context.Errors.Add(S["The scheme '{0}' needs a name, four '#rrggbb' colors (Primary, Deep, Ink, Accent), and '#rrggbb' optional colors.", scheme.Id]);

            return position;
        }

        if (!AdminSchemeStore.HasValidTypography(scheme))
        {
            context.Errors.Add(S["The scheme '{0}' has a FontFamily or HeadingFontFamily that is not a font name or a list of them separated by commas (letters, digits, spaces, dashes, underscores and dots), or a FontScale (0.875 to 1.125), RadiusScale (0 to 1.5) or Density (0.75 to 1.25) out of range.", scheme.Id]);

            return position;
        }

        await _store.SaveAsync(scheme);

        return position;
    }

    // A missing or null property is no value; one of another type is added to 'invalid'.
    private static T? Read<T>(JsonObject entry, string name, List<string> invalid)
        where T : struct
    {
        var node = entry[name];

        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue<T>(out var result))
        {
            return result;
        }

        invalid.Add(name);

        return null;
    }

    private static string ReadString(JsonObject entry, string name, List<string> invalid)
    {
        var node = entry[name];

        if (node is null)
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var result))
        {
            return result;
        }

        invalid.Add(name);

        return null;
    }
}
