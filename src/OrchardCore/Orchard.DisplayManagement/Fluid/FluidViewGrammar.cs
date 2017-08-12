using Fluid;
using Irony.Parsing;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewGrammar : FluidGrammar
    {
        public FluidViewGrammar() : base()
        {
            var RenderBody = ToTerm("render_body");

            var RenderSection = new NonTerminal("render_section");
            RenderSection.Rule = "render_section" + Identifier + FilterArguments;
            RenderSection.Rule |= "render_section" + Identifier;

            var RenderTitleSegments = new NonTerminal("page_title");
            RenderTitleSegments.Rule = "page_title" + Expression + FilterArguments;
            RenderTitleSegments.Rule |= "page_title" + Expression;

            var Display = new NonTerminal("display");
            Display.Rule = "display" + Expression;

            var Zone = new NonTerminal("zone");
            Zone.Rule = "zone" + FilterArguments;
            var ZoneEnd = ToTerm("endzone");

            var ContentLink = new NonTerminal("a");
            ContentLink.Rule = "a" + FilterArguments;
            var ContentLinkEnd = ToTerm("enda");

            var JavaScript = new NonTerminal("javascript");
            JavaScript.Rule = "javascript" + FilterArguments;
            var JavaScriptEnd = ToTerm("endjavascript");

            var Script = new NonTerminal("script");
            Script.Rule = "script" + FilterArguments;

            var Link = new NonTerminal("link");
            Link.Rule = "link" + FilterArguments;

            var Style = new NonTerminal("style");
            Style.Rule = "style" + FilterArguments;

            var Resources = new NonTerminal("resources");
            Resources.Rule = "resources" + FilterArguments;

            var Meta = new NonTerminal("meta");
            Meta.Rule = "meta" + FilterArguments;

            var Shape = new NonTerminal("shape");
            Shape.Rule = "shape" + FilterArguments;

            var Menu = new NonTerminal("menu");
            Menu.Rule = "menu" + FilterArguments;

            KnownTags.Rule |= RenderBody | RenderSection | RenderTitleSegments | Display
                | Zone | ZoneEnd | ContentLink | ContentLinkEnd | JavaScript | JavaScriptEnd
                | Script | Link | Style | Resources | Meta | Shape | Menu;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation(",",
                "render_section", "page_title", "display", "zone", "a", "javascript",
                "script", "link", "style", "resources", "meta", "shape", "menu");
        }
    }
}