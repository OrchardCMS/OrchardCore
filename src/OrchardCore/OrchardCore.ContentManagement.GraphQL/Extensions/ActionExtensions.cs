using System;
using System.Linq.Expressions;

namespace OrchardCore.ContentManagement.GraphQL
{
    public static class ActionExtensions
    {
        public static string NameOf<T>(this Expression<Action<T>> expression)
        {
            var member = (MemberExpression)expression.Body;
            return member.Member.Name;
        }
    }
}
