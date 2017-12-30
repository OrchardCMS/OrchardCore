namespace OrchardCore.Workflows.Services
{
    public abstract class EventActivity : Activity, IEvent
    {
        public virtual bool CanStartWorkflow => true;
    }
}