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

                case "endscript":
                    return BuildScriptStatement(null);

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
            if (tag?.Term.Name == "script")
            {
                foreach (var node in tag.ChildNodes[0].ChildNodes)
                {
                    var name = node.FindToken().ValueString;
                    if (name == "asp_name" || name == "asp_source")
                    {
                        // here we assume that the tag has no content, so it is self closed
                        return new ScriptStatement(BuildArgumentsExpression(tag.ChildNodes[0]), null);
                    }
                }

                EnterBlock(tag);
                return null;
            }

            if (_currentContext.Tag == null)
            {
                return null;
            }

            if (_currentContext.Tag.Term.Name != "script")
            {
                throw new ParseException($"Unexpected tag: ${tag.Term.Name} not matching script tag.");
            }

            var scriptStatement = new ScriptStatement(BuildArgumentsExpression(
                _currentContext.Tag.ChildNodes[0]), _currentContext.Statements);

            ExitBlock();
            return scriptStatement;
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