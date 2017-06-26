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

            KnownTags.Rule |= RenderBody | RenderSection | RenderTitleSegments | Script;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation("RenderSection", "RenderTitleSegments", "script");
        }
    }
}