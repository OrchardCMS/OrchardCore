using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class BooleanFieldIndexHandler : ContentFieldIndexHandler<BooleanField>
    {
        public override Task BuildIndexAsync(BooleanField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, field.Value, options);
            }

            return Task.CompletedTask;
        }
    }
}
