using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.AuditTrail.ViewModels;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventDriverBase : IAuditTrailEventDriver
    {
        public virtual Task DisplayFiltersAsync(DisplayFiltersContext context) => Task.CompletedTask;
        public virtual Task DisplayColumnsAsync(DisplayColumnsContext context) => Task.CompletedTask;
        public virtual Task DisplayColumnNamesAsync(DisplayColumnNamesContext context) => Task.CompletedTask;
        public virtual Task BuildViewModelAsync(AuditTrailEventViewModel viewModel) => Task.CompletedTask;
        public IStringLocalizer T { get; set; }
    }
}
