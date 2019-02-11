using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OrchardCore.ContentManagement.GraphQL.Options
{
    public class GraphQLContentPartOption : GraphQLContentPartOption<object>
    {
    }

    public class GraphQLContentPartOption<TSourceType>
    {
        public GraphQLContentPartOption()
        {
            Name = nameof(TSourceType);
        }

        public string Name { get; set; }
        public bool Collapse { get; set; }

        public bool Ignore { get; set; }
        public IList<string> IgnoredPropertyNames { get; set; }

        public GraphQLContentPartOption<TSourceType> IgnoreProperty(
            Expression<Action<TSourceType>> expression)
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
}