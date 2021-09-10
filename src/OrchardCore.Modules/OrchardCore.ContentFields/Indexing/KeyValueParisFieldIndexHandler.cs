using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class KeyValuePairsFieldIndexHandler : ContentFieldIndexHandler<KeyValuePairsField>
    {
        public override Task BuildIndexAsync(KeyValuePairsField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                foreach(var value in field.Values)
                {
                    context.DocumentIndex.Set(key, value.Key, options);
                    context.DocumentIndex.Set(key, value.Value, options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
