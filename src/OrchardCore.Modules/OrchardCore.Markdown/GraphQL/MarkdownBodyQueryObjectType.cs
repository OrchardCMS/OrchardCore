using GraphQL.Types;
using OrchardCore.Markdown.Model;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownBodyQueryObjectType : ObjectGraphType<MarkdownBodyPart>
    {
        public MarkdownBodyQueryObjectType()
        {
            Name = "MarkdownBodyPart";

            Field(x => x.Markdown);
            Field("html", x => ToHtml(x.Markdown), true).Description("the HTML representation of the markdown content");
        }

        private static string ToHtml(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown ?? "");
        }
    }
}
