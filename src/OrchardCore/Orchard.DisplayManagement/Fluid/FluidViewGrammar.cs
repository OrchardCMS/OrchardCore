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
            RenderSection.Rule = "RenderSection" + FilterArguments;

            var RenderTitleSegments = new NonTerminal("RenderTitleSegments");
            RenderTitleSegments.Rule = "RenderTitleSegments" + FilterArguments;

            var Display = new NonTerminal("Display");
            Display.Rule = "Display" + FilterArguments;

            var ClearAlternates = ToTerm("ClearAlternates");

            var SetMetadata = new NonTerminal("SetMetadata");
            SetMetadata.Rule = "SetMetadata" + FilterArguments;

            var Zone = new NonTerminal("zone");
            Zone.Rule = "zone" + FilterArguments;
            var ZoneEnd = ToTerm("endzone");

            var Script = new NonTerminal("script");
            Script.Rule = "script" + FilterArguments;
            var ScriptEnd = ToTerm("endscript");

            var Style = new NonTerminal("style");
            Style.Rule = "style" + FilterArguments;

            var Resources = new NonTerminal("resources");
            Resources.Rule = "resources" + FilterArguments;

            var Menu = new NonTerminal("menu");
            Menu.Rule = "menu" + FilterArguments;

            KnownTags.Rule |= RenderBody | RenderSection | RenderTitleSegments
                | Display | ClearAlternates | SetMetadata
                | Zone | ZoneEnd | Script | ScriptEnd | Style | Resources
                | Menu;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation("RenderSection", "RenderTitleSegments",
                "Display", "SetMetadata",
                "zone", "script", "style", "resources",
                "menu");
        }
    }
}