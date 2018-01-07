namespace OrchardCore.Workflows.Activities
{
    public interface IEvent : IActivity
    {
        /// <summary>
        /// Returns a value whether the event can cause a workflow to start.
        /// </summary>
        bool CanStartWorkflow { get; }
    }
}