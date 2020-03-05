using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class TextFieldIndexHandler : ContentFieldIndexHandler<TextField>
    {
        public override Task BuildIndexAsync(TextField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                context.DocumentIndex.Set(key, field.Text, options);
            }

            return Task.CompletedTask;
        }
    }
}
