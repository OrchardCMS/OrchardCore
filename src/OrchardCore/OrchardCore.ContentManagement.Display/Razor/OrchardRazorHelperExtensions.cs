using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Razor;

public static class OrchardRazorHelperExtensions
{
    public static async Task<IHtmlContent> DisplayAsync(this IOrchardDisplayHelper orchardDisplayHelper, ContentItem content, string displayType = "", string groupId = "", IUpdateModel updater = null)
    {
        var displayManager = orchardDisplayHelper.HttpContext.RequestServices.GetService<IContentItemDisplayManager>();
        var shape = await displayManager.BuildDisplayAsync(content, updater, displayType, groupId);

        return await orchardDisplayHelper.DisplayHelper.ShapeExecuteAsync(shape);
    }
}
