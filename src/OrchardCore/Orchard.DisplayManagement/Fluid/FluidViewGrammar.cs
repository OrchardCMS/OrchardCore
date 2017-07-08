using Fluid;
using Irony.Parsing;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewGrammar : FluidGrammar
    {
        public FluidViewGrammar() : base()
        {
            var RenderBody = ToTerm("RenderBody");

            var RenderSection = new NonTerminal("RenderSection");
            RenderSection.Rule = "RenderSection" + Identifier + FilterArguments;
            RenderSection.Rule |= "RenderSection" + Identifier;

            var RenderTitleSegments = new NonTerminal("RenderTitleSegments");
            RenderTitleSegments.Rule = "RenderTitleSegments" + Expression + FilterArguments;
            RenderTitleSegments.Rule |= "RenderTitleSegments" + Expression;

            var Display = new NonTerminal("Display");
            Display.Rule = "Display" + Expression;

            var ClearAlternates = new NonTerminal("ClearAlternates");
            ClearAlternates.Rule = "ClearAlternates" + Expression;

            var SetMetadata = new NonTerminal("SetMetadata");
            SetMetadata.Rule = "SetMetadata" + Expression + FilterArguments;

            var RemoveItem = new NonTerminal("RemoveItem");
            RemoveItem.Rule = "RemoveItem" + Expression + Expression;

            var SetProperty = new NonTerminal("SetProperty");
            SetProperty.Rule = "SetProperty" + Expression + ToTerm(":") + Expression + "=" + Expression;

            var Zone = new NonTerminal("zone");
            Zone.Rule = "zone" + FilterArguments;
            var ZoneEnd = ToTerm("endzone");

            var Script = new NonTerminal("script");
            Script.Rule = "script" + FilterArguments;
            var ScriptEnd = ToTerm("endscript");

            var ContentLink = new NonTerminal("a");
            ContentLink.Rule = "a" + FilterArguments;
            var ContentLinkEnd = ToTerm("enda");

            var Link = new NonTerminal("link");
            Link.Rule = "link" + FilterArguments;

            var Style = new NonTerminal("style");
            Style.Rule = "style" + FilterArguments;

            var Resources = new NonTerminal("resources");
            Resources.Rule = "resources" + FilterArguments;

            var Menu = new NonTerminal("menu");
            Menu.Rule = "menu" + FilterArguments;

            KnownTags.Rule |= RenderBody | RenderSection | RenderTitleSegments
                | Display | ClearAlternates | SetMetadata | RemoveItem | SetProperty
                | Zone | ZoneEnd | ContentLink | ContentLinkEnd | Link | Script | ScriptEnd | Style | Resources
                | Menu;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation("RenderSection", "RenderTitleSegments", "Display",
                "ClearAlternates", "SetMetadata", "RemoveItem", "SetProperty",
                "zone", "a", "link", "script", "style", "resources", "menu");
        }
    }
}