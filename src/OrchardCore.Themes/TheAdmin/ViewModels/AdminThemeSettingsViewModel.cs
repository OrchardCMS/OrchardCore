using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.ViewModels;

public class AdminThemeSettingsViewModel
{
    public string DefaultScheme { get; set; }

    [BindNever]
    public IReadOnlyList<AdminScheme> Schemes { get; set; } = [];
}
