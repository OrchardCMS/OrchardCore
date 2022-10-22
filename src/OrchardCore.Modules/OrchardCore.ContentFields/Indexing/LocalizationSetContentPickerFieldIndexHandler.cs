using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;
using OrchardCore.Modules;

namespace OrchardCore.ContentFields.Indexing
{
    [RequireFeatures("OrchardCore.ContentLocalization")]
    public class LocalizationSetContentPickerFieldIndexHandler : ContentFieldIndexHandler<LocalizationSetContentPickerField>
    {
        public override Task BuildIndexAsync(LocalizationSetContentPickerField field, BuildFieldIndexContext context)
        {
            var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;

            if (field.LocalizationSets.Length > 0)
            {
                foreach (var localizationSet in field.LocalizationSets)
                {
                    foreach (var key in context.Keys)
                    {
                        context.DocumentIndex.Set(key, localizationSet, options);
                    }
                }
            }
            else
            {
                foreach (var key in context.Keys)
                {
                    context.DocumentIndex.Set(key, "NULL", options);
                }
            }

            return Task.CompletedTask;
        }
    }
}
