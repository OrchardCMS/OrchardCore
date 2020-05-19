using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Shell.Scope;
using OrchardCore.Modules;

namespace OrchardCore.Lists
{
    [Feature("OrchardCore.Lists")]
    public class ListContentsAdminListShapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("ContentsAdminListZones")
                .OnCreated(async context =>
                {
                    var shape = (dynamic)context.Shape;
                    var options = shape.Options as ContentOptions;

                    var S = ShellScope.Services.GetRequiredService<IStringLocalizer<ListContentsAdminListShapes>>();
                    if (options != null)
                    {
                        options.ContentTypeOptions.Insert(1, new SelectListItem() { Text = S["Special list content type"], Value = "Blog" });
                    }
                    var listFilter = (IShape)await context.New.ContentsAdminList__ListPartFilter();
                    listFilter.Metadata.Prefix = "ListPart";
                    shape.Actions.Add(listFilter, ":30");
                });
        }
    }
}
