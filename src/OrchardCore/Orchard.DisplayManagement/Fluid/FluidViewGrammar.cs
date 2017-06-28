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

            var Script = new NonTerminal("script");
            Script.Rule = "script" + FilterArguments;
            var ScriptEnd = ToTerm("endscript");

            var Style = new NonTerminal("style");
            Style.Rule = "style" + FilterArguments;

            var Resources = new NonTerminal("resources");
            Resources.Rule = "resources" + FilterArguments;

            KnownTags.Rule |= RenderBody | RenderSection | RenderTitleSegments
                | Script | ScriptEnd
                | Style | Resources;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation("RenderSection", "RenderTitleSegments",
                "script", "style", "resources");
        }
    }
}