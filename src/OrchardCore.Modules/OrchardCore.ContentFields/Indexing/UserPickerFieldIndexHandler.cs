using System.Threading.Tasks;
using OrchardCore.ContentFields.Fields;
using OrchardCore.Indexing;

namespace OrchardCore.ContentFields.Indexing
{
    public class UserPickerFieldIndexHandler : ContentFieldIndexHandler<UserPickerField>
    {
        public override Task BuildIndexAsync(UserPickerField field, BuildFieldIndexContext context)
        {
            var options = DocumentIndexOptions.Keyword | DocumentIndexOptions.Store;

            if (field.UserIds.Length > 0)
            {
                foreach (var userId in field.UserIds)
                {
                    foreach (var key in context.Keys)
                    {
                        context.DocumentIndex.Set(key, userId, options);
                    }
                }

                var userNames = field.GetUserNames();
                foreach (var userName in userNames)
                {
                    foreach (var key in context.Keys)
                    {
                        context.DocumentIndex.Set(key, userName, options);
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
