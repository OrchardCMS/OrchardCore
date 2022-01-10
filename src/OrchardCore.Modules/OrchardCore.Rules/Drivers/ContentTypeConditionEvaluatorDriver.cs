using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Drivers
{
    /// <summary>
    /// Saves references to content types which have been displayed during a request.
    /// </summary>
    public class ContentTypeConditionEvaluatorDriver : ContentDisplayDriver, IConditionEvaluator
    {
        private static ValueTask<bool> True => new ValueTask<bool>(true);
        private static ValueTask<bool> False => new ValueTask<bool>(false);

        private readonly IConditionOperatorResolver _operatorResolver;

        // Hashset to prevent duplicate entries, but comparison is done by the comparers.
        private readonly HashSet<string> _contentTypes = new HashSet<string>();

        public ContentTypeConditionEvaluatorDriver(IConditionOperatorResolver operatorResolver)
        {
            _operatorResolver = operatorResolver;
        }

        public override Task<IDisplayResult> DisplayAsync(ContentItem contentItem, BuildDisplayContext context)
        {
            // Do not include Widgets or any display type other than detail.
            if (context.DisplayType == "Detail" && !context.Shape.TryGetProperty(nameof(ContentTypeSettings.Stereotype), out string _))
            {
                _contentTypes.Add(contentItem.ContentType);
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        public ValueTask<bool> EvaluateAsync(Condition condition)
            => EvaluateAsync(condition as ContentTypeCondition);

        private ValueTask<bool> EvaluateAsync(ContentTypeCondition condition)
        {
            var operatorComparer = _operatorResolver.GetOperatorComparer(condition.Operation);
            foreach (var contentType in _contentTypes)
            {
                if (operatorComparer.Compare(condition.Operation, contentType, condition.Value))
                {
                    return True;
                }
            }

            return False;
        }
    }
}
