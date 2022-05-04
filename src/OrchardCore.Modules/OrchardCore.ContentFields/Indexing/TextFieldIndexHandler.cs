using System.Linq;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class TextFieldIndexHandler : ContentFieldIndexHandler<TextField>
    {
        private const int MaxStringLength = 32766;

        public override Task BuildIndexAsync(TextField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();

            foreach (var key in context.Keys)
            {
                foreach (var chunk in field.Text.Chunk(MaxStringLength))
                {
                    context.DocumentIndex.Set(key, new string(chunk), options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
