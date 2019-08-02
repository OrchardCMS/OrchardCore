using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp.Web.Resolvers;

namespace OrchardCore.Media
{
    public interface IMediaFileResolverFactory
    {
        Task<IImageResolver> GetAsync(HttpContext context);
    }
}
