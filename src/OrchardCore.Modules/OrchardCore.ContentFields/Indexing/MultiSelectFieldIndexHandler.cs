using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class MultiSelectFieldIndexHandler : ContentFieldIndexHandler<MultiSelectField>
    {
        public override Task BuildIndexAsync(MultiSelectField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, string.Join(",", field.Values), options);
            }

            return Task.CompletedTask;
        }
    }
}
