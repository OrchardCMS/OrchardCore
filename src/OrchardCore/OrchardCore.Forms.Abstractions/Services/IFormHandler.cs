using System;
using System.Threading.Tasks;
using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.Services
{
    public interface IFormHandler
    {
        Task SubmittedAsync(FormSubmittedContext context);
    }
}
