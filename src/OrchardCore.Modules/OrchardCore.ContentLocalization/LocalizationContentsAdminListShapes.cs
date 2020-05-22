using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Localization;
using OrchardCore.Modules;

namespace OrchardCore.ContentLocalization
{
    [Feature("OrchardCore.ContentLocalization")]
    public class LocalizationContentsAdminListShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("ContentsAdminListHeader")
                .OnCreated(async context =>
                {
                    var shape = (dynamic)context.Shape;

                    var S = ShellScope.Services.GetRequiredService<IStringLocalizer<LocalizationContentsAdminListShapes>>();
                    var localizationService = ShellScope.Services.GetRequiredService<ILocalizationService>();
                    var supportedCultures = await localizationService.GetSupportedCulturesAsync();
                    var cultures = new List<SelectListItem>
                    {
                        new SelectListItem() { Text = S["All cultures"], Value = "" }
                    };
                    cultures.AddRange(supportedCultures.Select(culture => new SelectListItem() { Text = culture, Value = culture }));

                    var localizationShape = await context.ShapeFactory.CreateAsync<LocalizationContentAdminFilterViewModel>("ContentsAdminList__LocalizationPartFilter", m =>
                    {
                        m.Cultures = cultures;
                    });
                    localizationShape.Metadata.Prefix = "Localization";

                    shape.Actions.Add(localizationShape, ":20");
                });
        }
    }
}
