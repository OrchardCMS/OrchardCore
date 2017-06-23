using System;
using System.Collections.Generic;
using System.Linq;
using Fluid;
using Fluid.Ast;
using Irony.Parsing;
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
            var sectionName = string.Empty;
            if (tag.ChildNodes.Count > 0 && tag.ChildNodes[1].Term.Name.Equals("identifier"))
            {
                sectionName = tag.ChildNodes[1].FindToken().ValueString;
            }

            var required = false;
            if (tag.ChildNodes.Count > 1 && tag.ChildNodes[2].Term.Name.Equals("filterArguments"))
            {
                //var arguments = tag.ChildNodes[2].ChildNodes.Select(BuildFilterArgument).ToArray();

                //foreach (var argument in arguments)
                //{
                //    if (argument.Name.Equals("required"))
                //    {
                //        required = argument.Expression.EvaluateAsync(new TemplateContext())
                //            .GetAwaiter().GetResult().ToBooleanValue();
                //    }
                //}

                // faster for a simple value
                foreach (var argument in tag.ChildNodes[2].ChildNodes)
                {
                    if (argument.ChildNodes.Count > 1 && argument.ChildNodes[0].FindToken().ValueString.Equals("required"))
                    {
                        required = Convert.ToBoolean(argument.ChildNodes[1].FindToken().ValueString);
                    }
                }
            }

            return new RenderSectionStatement(sectionName, required);
        }
    }
}