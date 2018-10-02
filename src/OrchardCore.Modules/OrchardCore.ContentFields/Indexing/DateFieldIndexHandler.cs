using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class DateFieldIndexHandler : ContentFieldIndexHandler<DateField>
    {
        public override Task BuildIndexAsync(DateField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            context.DocumentIndex.Set(context.Key, field.Value, options);

            return Task.CompletedTask;
        }
    }
}
