using System.Threading.Tasks;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Workflows.Abstractions.Display;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Models;
using OrchardCore.Workflows.ViewModels;

namespace OrchardCore.Workflows.Drivers
{
    public class EvaluateExpressionTaskDisplay : ActivityDisplayDriver<EvaluateExpressionTask>
    {
        public override IDisplayResult Display(EvaluateExpressionTask activity)
        {
            return Combine(
                Shape("EvaluateExpressionTask_Fields_Thumbnail", activity).Location("Thumbnail", "Content"),
                Shape("EvaluateExpressionTask_Fields_Design", activity).Location("Design", "Content")
            );
        }

        public override IDisplayResult Edit(EvaluateExpressionTask activity)
        {
            return Shape<EvaluateExpressionTaskViewModel>("EvaluateExpressionTask_Fields_Edit", model =>
            {
                model.Expression = activity.Expression.Expression;
            }).Location("Content");
        }

        public async override Task<IDisplayResult> UpdateAsync(EvaluateExpressionTask activity, IUpdateModel updater)
        {
            var viewModel = new EvaluateExpressionTaskViewModel();
            if (await updater.TryUpdateModelAsync(viewModel, Prefix))
            {
                activity.Expression = new WorkflowExpression<object>(viewModel.Expression);
            }
            return Edit(activity);
        }
    }
}
