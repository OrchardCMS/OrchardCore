using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Themes.Schemes.ViewModels;

/// <summary>
/// The editor of a scheme.
/// </summary>
public class ThemeSchemeViewModel
{
    private const string Color = "^#[0-9a-fA-F]{6}$";
    private const string OptionalColor = "^(#[0-9a-fA-F]{6})?$";

    [BindNever]
    public string ThemeId { get; set; }

    [BindNever]
    public string ThemeName { get; set; }

    public string Id { get; set; }

    [Required]
    [StringLength(60)]
    public string Name { get; set; }

    [Range(0, 9999)]
    public int Position { get; set; }

    [Required]
    [RegularExpression(Color)]
    public string Primary { get; set; } = "#2a81bb";

    [Required]
    [RegularExpression(Color)]
    public string Deep { get; set; } = "#144c71";

    [Required]
    [RegularExpression(Color)]
    public string Ink { get; set; } = "#081b26";

    [Required]
    [RegularExpression(Color)]
    public string Accent { get; set; } = "#5aa4ef";

    public bool LightMenu { get; set; }

    [Range(0, 360)]
    public double? Hue { get; set; }

    [Range(0, 1)]
    public double? Saturation { get; set; }

    [RegularExpression(OptionalColor)]
    public string OnPrimary { get; set; }

    [RegularExpression(OptionalColor)]
    public string Page { get; set; }

    [RegularExpression(OptionalColor)]
    public string Toolbar { get; set; }

    [RegularExpression(OptionalColor)]
    public string Tint { get; set; }

    [RegularExpression(OptionalColor)]
    public string TintDark { get; set; }

    [StringLength(ThemeSchemeTypography.MaxFontFamilyLength)]
    public string FontFamily { get; set; }

    [StringLength(ThemeSchemeTypography.MaxFontFamilyLength)]
    public string HeadingFontFamily { get; set; }

    [Range(ThemeSchemeTypography.MinFontScale, ThemeSchemeTypography.MaxFontScale)]
    public double FontScale { get; set; } = ThemeSchemeTypography.DefaultFontScale;

    [Range(ThemeSchemeTypography.MinRadiusScale, ThemeSchemeTypography.MaxRadiusScale)]
    public double RadiusScale { get; set; } = ThemeSchemeTypography.DefaultRadiusScale;

    [Range(ThemeSchemeTypography.MinDensity, ThemeSchemeTypography.MaxDensity)]
    public double Density { get; set; } = ThemeSchemeTypography.DefaultDensity;

    public bool IsNew => string.IsNullOrEmpty(Id);
}
