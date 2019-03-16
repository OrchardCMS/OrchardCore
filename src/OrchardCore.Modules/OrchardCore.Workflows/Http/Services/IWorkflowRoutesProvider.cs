using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Workflows.Http.Services
{
    public interface IWorkflowRoutesProvider
    {
        Task RegisterRoutesAsync();
    }
}
