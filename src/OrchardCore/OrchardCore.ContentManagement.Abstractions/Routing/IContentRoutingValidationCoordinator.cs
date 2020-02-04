using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Routing
{
    public interface IContentRoutingValidationCoordinator
    {
        Task<bool> IsPathUniqueAsync(string path, string contentItemId);
    }
}
