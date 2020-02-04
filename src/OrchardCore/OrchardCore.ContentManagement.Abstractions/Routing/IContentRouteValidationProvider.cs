using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Routing
{
    public interface IContentRouteValidationProvider
    {
        Task<bool> IsPathUniqueAsync(string path, string contentItemId);
    }
}
