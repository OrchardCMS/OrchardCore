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

                case "zone":
                    return BuildZoneStatement(tag);

                case "endzone":
                    return BuildZoneStatement(null);

                case "script":
                    return BuildScriptStatement(tag);

                case "endscript":
                    return BuildScriptStatement(null);

                case "style":
                    return BuildStyleStatement(tag);

                case "resources":
                    return BuildResourcesStatement(tag);

                case "menu":
                    return BuildShapeStatement(tag);

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

        private RenderTitleSegmentsStatement BuildRenderTitleSegmentsStatement(ParseTreeNode tag)
        {
            return new RenderTitleSegmentsStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private DisplayStatement BuildDisplayStatement(ParseTreeNode tag)
        {
            return new DisplayStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private ClearAlternates BuildClearAlternatesStatement(ParseTreeNode tag)
        {
            return new ClearAlternates();
        }

        private SetMetadataStatement BuildSetMetadataStatement(ParseTreeNode tag)
        {
            return new SetMetadataStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private ZoneStatement BuildZoneStatement(ParseTreeNode tag)
        {
            if (tag != null)
            {
                if (tag?.Term.Name == "zone")
                {
                    EnterBlock(tag);
                    return null;
                }
            }
            else
            {
                if (_currentContext.Tag == null)
                {
                    return null;
                }

                if (_currentContext.Tag.Term.Name == "zone")
                {
                    var statement = new ZoneStatement(BuildArgumentsExpression(
                        _currentContext.Tag.ChildNodes[0]), _currentContext.Statements);

                    ExitBlock();
                    return statement;
                }
            }

            throw new ParseException($"Unexpected tag: ${_currentContext.Tag.Term.Name} not matching script tag.");
        }

        private ScriptStatement BuildScriptStatement(ParseTreeNode tag)
        {
            if (tag != null)
            {
                if (tag?.Term.Name == "script")
                {
                    foreach (var node in tag.ChildNodes[0].ChildNodes)
                    {
                        var name = node.FindToken().ValueString;
                        if (name == "asp_name" || name == "asp_source")
                        {
                            return new ScriptStatement(BuildArgumentsExpression(tag.ChildNodes[0]), null);
                        }
                    }

                    EnterBlock(tag);
                    return null;
                }
            }
            else
            {
                if (_currentContext.Tag == null)
                {
                    return null;
                }

                if (_currentContext.Tag.Term.Name == "script")
                {
                    var statement = new ScriptStatement(BuildArgumentsExpression(
                        _currentContext.Tag.ChildNodes[0]), _currentContext.Statements);

                    ExitBlock();
                    return statement;
                }
            }

            throw new ParseException($"Unexpected tag: ${_currentContext.Tag.Term.Name} not matching script tag.");
        }

        private StyleStatement BuildStyleStatement(ParseTreeNode tag)
        {
            return new StyleStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
        }

        private ResourcesStatement BuildResourcesStatement(ParseTreeNode tag)
        {
            return new ResourcesStatement(BuildArgumentsExpression(tag.ChildNodes[0]));
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