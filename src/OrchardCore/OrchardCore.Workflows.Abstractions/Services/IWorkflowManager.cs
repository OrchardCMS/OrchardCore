using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.Entities;

namespace OrchardCore.Workflows.Services
{
    public interface IWorkflowManager
    {
        /// <summary>
        /// Triggers a specific <see cref="IEvent"/>, and provides context if the event is
        /// actually executed.
        /// </summary>
        /// <param name="name">The type of the event to trigger, e.g. Publish.</param>
        /// <param name="target">The target entity the event is related to.</param>
        /// <param name="context">An object containing context for the event.</param>
        Task TriggerEvent(string name, IEntity target, Func<Dictionary<string, object>> context);
    }
}