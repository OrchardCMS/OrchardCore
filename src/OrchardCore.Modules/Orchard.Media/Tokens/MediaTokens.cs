using HandlebarsDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.Media.Models;

namespace Orchard.Media.Tokens
{
    public static class MediaTokens
    {
        public static void RegisterMediaTokens(this IHandlebars handlebars, IHttpContextAccessor httpContextAccessor)
        {
            // Renders an image media with optional dynamic profile settings
            handlebars.RegisterHelper("image", (output, context, arguments) =>
            {
                var services = httpContextAccessor.HttpContext.RequestServices;
                var contentManager = services.GetRequiredService<IContentManager>();
                var mediaFileStore = services.GetRequiredService<IMediaFileStore>();

                var contentItem = contentManager.GetAsync(arguments[0].ToString()).GetAwaiter().GetResult();
                var image = contentItem.As<ImagePart>();
                var imageUrl = mediaFileStore.GetPublicUrl(image.Path);

                if (image != null)
                {
                    output.Write("<img src=\"" + imageUrl + "\" />");
                }                
            });
        }
    }
}
