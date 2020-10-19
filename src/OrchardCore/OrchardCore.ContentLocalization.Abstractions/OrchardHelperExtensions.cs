using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContentLocalization
{
    /// <summary>
    /// Provides an extension methods for <see cref="IOrchardHelper"/>.
    /// </summary>
    public static class OrchardHelperExtensions
    {
        /// <summary>
        /// Gets the culture for a given <see cref="ContentItem"/>.
        /// </summary>
        /// <param name="orchardHelper">The <see cref="IOrchardHelper"/>.</param>
        /// <param name="contentItem">The <see cref="ContentItem"/> in which to get its culture.</param>
        /// <param name="defaultCulture">The <see cref="CultureInfo"/> the default culture it should fallback to.</param>
        /// <returns></returns>
        public async static Task<CultureInfo> GetContentCultureAsync(this IOrchardHelper orchardHelper, ContentItem contentItem, CultureInfo defaultCulture = CultureInfo.CurrentUICulture)
        {
            var contentManager = orchardHelper.HttpContext.RequestServices.GetService<IContentManager>();
            var cultureAspect = await contentManager.PopulateAspectAsync(contentItem, new CultureAspect(){ Culture = defaultCulture });

            return cultureAspect.Culture;
        }
    }
}
