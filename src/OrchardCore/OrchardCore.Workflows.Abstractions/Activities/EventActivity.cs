namespace OrchardCore.Workflows.Activities
{
    public abstract class EventActivity : Activity, IEvent
    {
        public virtual bool CanStartWorkflow => true;
    }
}