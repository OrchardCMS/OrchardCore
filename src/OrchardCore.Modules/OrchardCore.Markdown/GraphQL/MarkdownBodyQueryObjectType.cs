using GraphQL.Types;
using OrchardCore.Markdown.Model;

namespace OrchardCore.Markdown.GraphQL
{
    public class MarkdownBodyQueryObjectType : ObjectGraphType<MarkdownBodyPart>
    {
        public MarkdownBodyQueryObjectType()
        {
            Name = nameof(MarkdownBodyPart);

            Field("markdown", x => x.Markdown, nullable: true)
                .Description("the markdown value")
                .Type(new StringGraphType())
                ;

            Field("html", x => ToHtml(x.Markdown), nullable: true)
                .Description("the HTML representation of the markdown content")
                .Type(new StringGraphType())
                ;
        }

        private static string ToHtml(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown ?? "");
        }
    }
}
