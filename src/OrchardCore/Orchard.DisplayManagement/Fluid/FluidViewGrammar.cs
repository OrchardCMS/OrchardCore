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
            RenderSection.Rule = "rendersection" + FilterArguments;

            var RenderTitleSegments = new NonTerminal("render_title_segments");
            RenderTitleSegments.Rule = "render_title_segments" + FilterArguments;

            var Script = new NonTerminal("script");
            Script.Rule = "script" + FilterArguments;

            KnownTags.Rule |= RenderBody | RenderSection | RenderTitleSegments | Script;

            // Prevent the text from being added in the parsed tree.
            // Only Identifier and Arguments will be in the tree.
            MarkPunctuation("rendersection", "render_title_segments", "script");
        }
    }
}