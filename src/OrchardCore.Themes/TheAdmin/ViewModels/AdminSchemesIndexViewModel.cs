using OrchardCore.Admin;
using OrchardCore.Themes.TheAdmin.Models;

namespace OrchardCore.Themes.TheAdmin.ViewModels;

public class AdminSchemesIndexViewModel
{
    public AdminListContext<AdminScheme> List { get; set; }
}
