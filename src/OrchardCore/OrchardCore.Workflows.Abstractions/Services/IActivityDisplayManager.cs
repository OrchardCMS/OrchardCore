using System.Threading.Tasks;
using OrchardCore.DisplayManagement;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;

namespace OrchardCore.Workflows.Services
{
    public interface IActivityDisplayManager : IDisplayManager<IActivity>
    {
    }
}
