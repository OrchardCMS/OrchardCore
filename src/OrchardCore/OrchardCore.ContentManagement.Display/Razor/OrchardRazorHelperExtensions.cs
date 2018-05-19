using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Razor;

namespace OrchardCore.ContentManagement
{
    public static class OrchardRazorHelperExtensions
    {
        public static async Task<IHtmlContent> DisplayAsync(this OrchardRazorHelper razorHelper, ContentItem content, string displayType = "", string groupId = "", IUpdateModel updater = null)
        {
            var displayManager = razorHelper.HttpContext.RequestServices.GetService<IContentItemDisplayManager>();
            var shape = await displayManager.BuildDisplayAsync(content, updater, displayType, groupId);

            return await razorHelper.DisplayHelper.ShapeExecuteAsync(shape);
        }
    }
}
