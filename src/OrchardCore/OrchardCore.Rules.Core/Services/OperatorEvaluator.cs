using System;
using OrchardCore.Rules.Models;

namespace OrchardCore.Rules.Services
{
    public abstract class OperatorComparer<T> : IOperatorComparer
    {
        bool IOperatorComparer.Compare(object a, object b)
            => Compare((T)a, (T)b);
        public abstract bool Compare(T a, T b);
    }

    public class StringEqualsOperatorComparer : OperatorComparer<string>
    {
        public override bool Compare(string a, string b)
            => String.Equals(a, b, StringComparison.OrdinalIgnoreCase);
    }
}