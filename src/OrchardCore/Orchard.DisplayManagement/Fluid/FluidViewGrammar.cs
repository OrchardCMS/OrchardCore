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

            var AddTagHelper = new NonTerminal("AddTagHelper");
            AddTagHelper.Rule = "AddTagHelper" + Term + "," + Term;

            var ContentLink = new NonTerminal("a");
            ContentLink.Rule = "a" + FilterArguments | "a" + FilterArguments + "/";
            var ContentLinkEnd = ToTerm("enda");

            var Script = new NonTerminal("script");
            Script.Rule = "script" + FilterArguments | "script" + FilterArguments + "/";
            var ScriptEnd = ToTerm("endscript");

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
                | Zone | ZoneEnd | AddTagHelper | ContentLink | ContentLinkEnd
                | Script | ScriptEnd | Link | Style | Resources | Meta
                | Shape | Menu;

            var SelfClosed = new NonTerminal("self_closed");
            Tag.Rule |= ToTerm("{%") + KnownTags + SelfClosed + ToTerm("%}");
            SelfClosed.Rule = Empty | "/";

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation(",",
                "self_closed", "render_section", "page_title", "display", "zone",
                "AddTagHelper", "a", "script", "link", "style", "resources", "meta",
                "shape", "menu");
        }
    }
}