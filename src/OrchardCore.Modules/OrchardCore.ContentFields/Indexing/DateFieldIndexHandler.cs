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

            foreach (var key in context.Keys)
            {
                if (field.Value != null)
                {
                    context.DocumentIndex.Set(key, field.Value, options);
                }
                else
                {
                    context.DocumentIndex.Set(key, "NULL", options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
