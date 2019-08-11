using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class DateTimeFieldIndexHandler : ContentFieldIndexHandler<DateTimeField>
    {
        public override Task BuildIndexAsync(DateTimeField field, BuildFieldIndexContext context)
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
