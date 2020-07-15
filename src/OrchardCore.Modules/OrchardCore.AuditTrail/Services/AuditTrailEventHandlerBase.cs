using Microsoft.Extensions.Localization;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.DisplayManagement.Shapes;
using System.Threading.Tasks;

namespace OrchardCore.AuditTrail.Services
{
    public class AuditTrailEventHandlerBase : IAuditTrailEventHandler
    {
        public virtual Task CreateAsync(AuditTrailCreateContext context) => Task.CompletedTask;
        public virtual void Filter(QueryFilterContext context) { }
        public virtual Task DisplayFilterAsync(DisplayFilterContext context) => Task.CompletedTask;
        public virtual Task DisplayAdditionalColumnsAsync(DisplayAdditionalColumnsContext context) => Task.CompletedTask;
        public virtual Task DisplayAdditionalColumnNamesAsync(Shape display) => Task.CompletedTask;

        public IStringLocalizer T { get; set; }
    }
}
