using System.Collections.Generic;
using System.Linq;
using Fluid;
using Fluid.Ast;
using Fluid.Tags;
using Irony.Parsing;
using OrchardCore.Liquid.Ast;

namespace OrchardCore.DisplayManagement.Liquid.Tags
{
    public class NamedHelperTag : ITag
    {
        public static readonly Dictionary<string, string> DefaultArguments = new Dictionary<string, string>();

        public BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.FilterArguments;
        }

        public Statement Parse(ParseTreeNode node, ParserContext context)
        {
            return new HelperStatement(new ArgumentsExpression(BuildArguments(node)), node.Term.Name);
        }

        public static FilterArgument[] BuildArguments(ParseTreeNode node)
        {
            var arguments = node.ChildNodes[0].ChildNodes.Select(DefaultFluidParser.BuildFilterArgument).ToArray();

            var defaultArgument = node.ChildNodes[0].ChildNodes[0].ChildNodes[0];

            if (defaultArgument.Term.Name != "identifier")
            {
                if (DefaultArguments.TryGetValue(node.Term.Name, out var name))
                {
                    foreach (var argument in arguments)
                    {
                        if (argument.Name == name)
                        {
                            return arguments;
                        }
                    }

                    arguments[0] = new FilterArgument(name, DefaultFluidParser.BuildTermExpression(defaultArgument));
                }
            }

            return arguments;
        }

        public static void RegisterDefaultArgument(string tagName, string argumentName)
        {
            DefaultArguments[tagName] = argumentName;
        }
    }

    public class NamedHelperBlock : CustomBlock
    {
        public override BnfTerm GetSyntax(FluidGrammar grammar)
        {
            return grammar.FilterArguments;
        }

        public override Statement Parse(ParseTreeNode node, ParserContext context)
        {
            var tag = context.CurrentBlock.Tag;
            var statements = context.CurrentBlock.Statements;

            var arguments = NamedHelperTag.BuildArguments(tag);
            return new HelperStatement(new ArgumentsExpression(arguments), tag.Term.Name, statements);
        }

        public static void RegisterDefaultArgument(string tagName, string argumentName)
        {
            NamedHelperTag.RegisterDefaultArgument(tagName, argumentName);
        }
    }
}