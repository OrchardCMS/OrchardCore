using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class MultiTextFieldIndexHandler : ContentFieldIndexHandler<MultiTextField>
    {
        public override Task BuildIndexAsync(MultiTextField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                foreach (var value in field.Values)
                {
                    context.DocumentIndex.Set(key, value, options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
