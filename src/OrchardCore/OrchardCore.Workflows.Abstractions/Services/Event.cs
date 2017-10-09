namespace OrchardCore.Workflows.Services
{
    public abstract class Event : Activity, IEvent
    {
        public virtual bool CanStartWorkflow => false;
    }
}