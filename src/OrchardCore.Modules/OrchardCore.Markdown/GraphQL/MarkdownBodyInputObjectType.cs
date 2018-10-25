using OrchardCore.Apis.GraphQL.Queries;
using OrchardCore.Markdown.Model;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownBodyInputObjectType : QueryArgumentObjectGraphType<MarkdownBodyPart>
    {
        public MarkdownBodyInputObjectType()
        {
            Name = "MarkdownBodyInput";

            AddInputField("Markdown", x => x.Markdown, true);
        }
    }
}
