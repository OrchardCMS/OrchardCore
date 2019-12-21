using System.Threading.Tasks;
using OrchardCore.Media.Events;

namespace OrchardCore.Media
{
    public interface IMediaStreamService
    {
        Task<OutputStream> Preprocess( MediaCreatingContext mediaCreatingContext);
    }
}
