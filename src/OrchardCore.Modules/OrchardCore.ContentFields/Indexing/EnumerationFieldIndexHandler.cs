using System;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class EnumerationFieldIndexHandler : ContentFieldIndexHandler<EnumerationField>
    {
        public override Task BuildIndexAsync(EnumerationField field, BuildFieldIndexContext context)
        {
            var editor = context.ContentPartFieldDefinition.Settings.ToObject<Settings.EnumerationFieldSettings>().Editor;

            //if we have single value
            if (editor == null || editor == "RadioButtonList")
            {
                var options = context.Settings.ToOptions();
                context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(field.Value, DocumentIndex.Types.Text, options));
            }
            else {
                var options = context.Settings.ToOptions();
                context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(String.Join(";", field.SelectedValues), DocumentIndex.Types.Text, options));
            }

            return Task.CompletedTask;
        }
    }
}
