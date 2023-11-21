namespace OrchardCore.Workflows.Activities;

public abstract class TaskActivity : Activity, ITask
{
}

public abstract class TaskActivity<TActivity> : TaskActivity where TActivity : ITask
{
    // The technical name of the activity. Within a workflow definition, activities make use of this name.
    public override string Name => typeof(TActivity).Name;
}
