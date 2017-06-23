using Fluid;
using Irony.Parsing;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewGrammar : FluidGrammar
    {
        public FluidViewGrammar() : base()
        {
            var RenderBody = ToTerm("renderbody");

            var RenderSection = new NonTerminal("rendersection");
            RenderSection.Rule = "rendersection" + Identifier + FilterArguments;

            KnownTags.Rule |= RenderBody | RenderSection;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation("rendersection");
        }
    }
}