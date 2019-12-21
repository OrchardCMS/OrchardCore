using System.Threading.Tasks;
using OrchardCore.Media.Events;

namespace OrchardCore.Media
{
    public interface IMediaStreamService
    {
        Task Preprocess( MediaCreatingContext mediaCreatingContext);
    }
}
