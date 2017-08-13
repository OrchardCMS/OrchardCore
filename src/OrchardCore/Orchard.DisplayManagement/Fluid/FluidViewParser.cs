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
                case "render_body":
                    return BuildRenderBodyStatement(tag);

                case "render_section":
                    return BuildRenderSectionStatement(tag);

                case "page_title":
                    return BuildRenderTitleSegmentsStatement(tag);

                case "display":
                    return BuildDisplayStatement(tag);

                case "zone":
                    EnterBlock(tag);
                    return null;

                case "endzone":
                    return BuildZoneStatement();

                case "shape":
                    return BuildShapeStatement(tag);

                case "link":
                case "style":
                case "resources":
                case "meta":
                case "script":
                    return BuildTagHelperStatement(tag);

                case "a":
                case "javascript":
                    EnterBlock(tag);
                    return null;

                case "enda":
                case "endjavascript":
                    return BuildTagHelperStatement(null);

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
            var name = tag.ChildNodes[0].Token.ValueString;
            var arguments = tag.ChildNodes.Count > 1 ? BuildArgumentsExpression(tag.ChildNodes[1]) : null;
            return new RenderSectionStatement(name, arguments);
        }

        private RenderTitleSegmentsStatement BuildRenderTitleSegmentsStatement(ParseTreeNode tag)
        {
            var segment = BuildExpression(tag.ChildNodes[0]);
            var expression = tag.ChildNodes.Count > 1 ? BuildArgumentsExpression(tag.ChildNodes[1]) : null;
            return new RenderTitleSegmentsStatement(segment, expression);
        }

        private DisplayStatement BuildDisplayStatement(ParseTreeNode tag)
        {
            var shape = BuildExpression(tag.ChildNodes[0]);
            return new DisplayStatement(shape);
        }

        private ZoneStatement BuildZoneStatement()
        {
            if (_currentContext?.Tag.Term.Name == "zone")
            {
                var statement = new ZoneStatement(BuildArgumentsExpression(
                    _currentContext.Tag.ChildNodes[0]), _currentContext.Statements);

                ExitBlock();
                return statement;
            }

            var unexpectedTag = _currentContext?.Tag.Term.Name ?? "undefined";
            throw new ParseException($"Unexpected tag: ${unexpectedTag} not matching zone tag.");
        }

        private ShapeStatement BuildShapeStatement(ParseTreeNode tag)
        {
            return new ShapeStatement(tag.ChildNodes[0].Token.ValueString, BuildArgumentsExpression(tag.ChildNodes[1]));
        }

        private TagHelperStatement BuildTagHelperStatement(ParseTreeNode tag)
        {
            if (tag != null)
            {
                var statement = new TagHelperStatement(tag.Term.Name,
                    BuildArgumentsExpression(tag.ChildNodes[0]), null);

                    return statement;
            }
            else
            {
                if (_currentContext == null)
                {
                    return null;
                }

                var statement = new TagHelperStatement(_currentContext.Tag.Term.Name,
                    BuildArgumentsExpression(_currentContext.Tag.ChildNodes[0]), _currentContext.Statements);

                ExitBlock();
                return statement;
            }
        }

        protected virtual ArgumentsExpression BuildArgumentsExpression(ParseTreeNode node)
        {
            return new ArgumentsExpression(node.ChildNodes.Select(BuildFilterArgument).ToArray());
        }
    }
}