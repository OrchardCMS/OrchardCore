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
                    return BuildRenderTitleSegmentsStatement(tag);

                case "Display":
                    return BuildDisplayStatement(tag);

                case "ClearAlternates":
                    return BuildClearAlternatesStatement(tag);

                case "SetMetadata":
                    return BuildSetMetadataStatement(tag);

                case "RemoveItem":
                    return BuildRemoveItemStatement(tag);

                case "SetProperty":
                    return BuildSetPropertyStatement(tag);

                case "zone":
                    EnterBlock(tag);
                    return null;

                case "endzone":
                    return BuildZoneStatement();

                case "link":
                case "style":
                case "resources":
                case "meta":
                    return BuildTagHelperStatement(tag);

                case "script":
                case "a":

                    if (tag.ChildNodes.Count > 1 && tag.ChildNodes[1].FindTokenAndGetText() == "/")
                    {
                        return BuildTagHelperStatement(tag);
                    }

                    EnterBlock(tag);
                    return null;

                case "endscript":
                case "enda":
                    return BuildTagHelperStatement(null);

                case "menu":
                    return BuildTagHelperStatement(tag, "shape");

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
            var identifier = tag.ChildNodes[0].Token.ValueString;
            var arguments = tag.ChildNodes.Count > 1 ? BuildArgumentsExpression(tag.ChildNodes[1]) : null;
            return new RenderSectionStatement(identifier, arguments);
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

        private ClearAlternates BuildClearAlternatesStatement(ParseTreeNode tag)
        {
            var shape = BuildExpression(tag.ChildNodes[0]);
            return new ClearAlternates(shape);
        }

        private SetMetadataStatement BuildSetMetadataStatement(ParseTreeNode tag)
        {
            var shape = BuildExpression(tag.ChildNodes[0]);
            return new SetMetadataStatement(shape, BuildArgumentsExpression(tag.ChildNodes[1]));
        }

        private RemoveItemStatement BuildRemoveItemStatement(ParseTreeNode tag)
        {
            var shape = BuildExpression(tag.ChildNodes[0]);
            var name = BuildExpression(tag.ChildNodes[1]);
            return new RemoveItemStatement(shape, name);
        }

        private SetPropertyStatement BuildSetPropertyStatement(ParseTreeNode tag)
        {
            var obj = BuildExpression(tag.ChildNodes[0]);
            var name = BuildExpression(tag.ChildNodes[1]);
            var value = BuildExpression(tag.ChildNodes[2]);

            return new SetPropertyStatement(obj, name, value);
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

        private TagHelperStatement BuildTagHelperStatement(ParseTreeNode tag, string baseType = null)
        {
            if (tag != null)
            {
                var statement = new TagHelperStatement(tag.Term.Name,
                    BuildArgumentsExpression(tag.ChildNodes[0]), null, baseType);

                    return statement;
            }
            else
            {
                if (_currentContext == null)
                {
                    return null;
                }

                var statement = new TagHelperStatement(_currentContext.Tag.Term.Name,
                    BuildArgumentsExpression(_currentContext.Tag.ChildNodes[0]), _currentContext.Statements, baseType);

                ExitBlock();
                return statement;
            }
        }

        private ShapeStatement BuildShapeStatement(ParseTreeNode tag)
        {
            return new ShapeStatement(tag.Term.Name , BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        protected virtual ArgumentsExpression BuildArgumentsExpression(ParseTreeNode node)
        {
            return new ArgumentsExpression(node.ChildNodes.Select(BuildFilterArgument).ToArray());
        }
    }
}