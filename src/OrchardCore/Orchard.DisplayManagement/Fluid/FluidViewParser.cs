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
                case "RenderBody":
                    return BuildRenderBodyStatement(tag);

                case "RenderSection":
                    return BuildRenderSectionStatement(tag);

                case "RenderTitleSegments":
                    return BuildRenderRenderTitleSegmentsStatement(tag);

                case "script":
                    return BuildScriptStatement(tag);

                case "style":
                    return BuildStyleStatement(tag);

                case "resources":
                    return BuildResourcesStatement(tag);

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
            return new RenderSectionStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private RenderTitleSegmentsStatement BuildRenderRenderTitleSegmentsStatement(ParseTreeNode tag)
        {
            return new RenderTitleSegmentsStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private ScriptStatement BuildScriptStatement(ParseTreeNode tag)
        {
            return new ScriptStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private StyleStatement BuildStyleStatement(ParseTreeNode tag)
        {
            return new StyleStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private ResourcesStatement BuildResourcesStatement(ParseTreeNode tag)
        {
            return new ResourcesStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        protected virtual ArgumentsExpression BuildArgumentsExpression(ParseTreeNode node)
        {
            return new ArgumentsExpression(node.ChildNodes.Select(BuildFilterArgument).ToArray());
        }
    }
}