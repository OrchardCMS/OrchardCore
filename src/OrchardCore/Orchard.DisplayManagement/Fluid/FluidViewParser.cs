using System;
using System.Linq;
using Fluid;
using Fluid.Ast;
using Irony.Parsing;
using Orchard.DisplayManagement.Fluid.Ast;
using Orchard.DisplayManagement.Fluid.Statements;

namespace Orchard.DisplayManagement.Fluid
{
    public class FluidViewParser : IronyFluidParser<FluidViewGrammar>
    {
        protected override Statement BuildTagStatement(ParseTreeNode node)
        {
            var tag = node.ChildNodes[0];

            switch (tag.Term.Name)
            {
                case "renderbody":
                    return BuildRenderBodyStatement(tag);

                case "rendersection":
                    return BuildRenderSectionStatement(tag);

                case "render_title_segments":
                    return BuildRenderRenderTitleSegmentsStatement(tag);

                case "script":
                    return BuildScriptStatement(tag);

                default:
                    return base.BuildTagStatement(node);
            }
        }

        private RenderBodyStatement BuildRenderBodyStatement(ParseTreeNode tag)
        {
            return new RenderBodyStatement();
        }

        private RenderSectionStatement BuildRenderSectionStatement(ParseTreeNode tag)
        {
            return new RenderSectionStatement(BuildFilterArgumentsExpression(tag));
        }

        private RenderTitleSegmentsStatement BuildRenderRenderTitleSegmentsStatement(ParseTreeNode tag)
        {
            return new RenderTitleSegmentsStatement(BuildFilterArgumentsExpression(tag));
        }

        private ScriptStatement BuildScriptStatement(ParseTreeNode tag)
        {
            FilterArgument[] parameters = tag.ChildNodes[0].ChildNodes.Select(BuildFilterArgument).ToArray();
            return new ScriptStatement(BuildFilterArgumentsExpression(tag));
        }

        protected virtual FilterArgumentsExpression BuildFilterArgumentsExpression(ParseTreeNode node)
        {
            return new FilterArgumentsExpression(node.ChildNodes[0].ChildNodes.Select(BuildFilterArgument).ToArray());
        }
    }
}