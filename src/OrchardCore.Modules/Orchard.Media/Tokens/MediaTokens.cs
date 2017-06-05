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
                var mediaFileStore = services.GetRequiredService<IMediaFileStore>();

                var url = arguments[0].ToString();
                var imageUrl = mediaFileStore.GetPublicUrl(url);

                if (imageUrl != null)
                {
                    output.Write("<img src=\"" + imageUrl + "\" />");
                }                
            });
        }
    }
}
