using System.Threading.Tasks;

namespace OrchardCore.Environment.Shell;

public interface IShellContextReleaseService
{
    void RequestRelease();

    void SuspendReleaseRequest();

    Task<bool> ProcessAsync();
}
