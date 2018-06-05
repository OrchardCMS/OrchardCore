using System;
using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class TimeFieldIndexHandler : ContentFieldIndexHandler<TimeField>
    {
        public override Task BuildIndexAsync(TimeField field, BuildFieldIndexContext context)
        {
            var options = context.Settings.ToOptions();
            DateTime? indexedValue = null;
            if (field.Value.HasValue)
            {
                indexedValue = new DateTime(field.Value.Value.Ticks);
            }
            context.DocumentIndex.Entries.Add(context.Key, new DocumentIndex.DocumentIndexEntry(indexedValue, DocumentIndex.Types.DateTime, options));

            return Task.CompletedTask;
        }
    }
}
