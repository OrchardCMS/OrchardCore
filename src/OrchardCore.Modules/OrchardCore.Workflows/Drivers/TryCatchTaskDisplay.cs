using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class TryCatchTaskDisplay : ActivityDisplayDriver<TryCatchTask, TryCatchTaskViewModel>
    {
        protected override void Map(TryCatchTask source, TryCatchTaskViewModel target)
        {
            target.ExceptionTypes = source.ExceptionTypes;
        }

        protected override void Map(TryCatchTaskViewModel source, TryCatchTask target)
        {
            target.ExceptionTypes = source.ExceptionTypes;
        }
    }
}
