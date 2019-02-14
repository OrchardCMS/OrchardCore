using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GraphQL;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLContentPartOption<TContentPart> : GraphQLContentPartOption where TContentPart : ContentPart
    {
        public GraphQLContentPartOption()
        {
            Name = nameof(TContentPart);
        }

        public GraphQLContentPartOption<TContentPart> IgnoreProperty<TReturnType>(
            Expression<Func<TContentPart, TReturnType>> expression)
        {
            string name;
            try
            {
                name = expression.NameOf();
            }
            catch
            {
                throw new ArgumentException(
                    $"Cannot infer a Property name from the expression: '{expression.Body.ToString()}' " +
                    $"on parent type: '{Name ?? GetType().Name}'.");
            }

            IgnoredPropertyNames.Add(name);

            return this;
        }
    }

    public class GraphQLContentPartOption
    {
        public string Name { get; set; }
        public bool Collapse { get; set; }

        public bool Ignore { get; set; }
        public IList<string> IgnoredPropertyNames { get; set; }
    }
}