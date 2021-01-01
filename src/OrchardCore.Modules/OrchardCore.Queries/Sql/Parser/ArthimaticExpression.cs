using System;

namespace OrchardCore.Queries.Sql.Parser
{
    public abstract class ArthimaticExpression : BinaryExpression
    {
        public ArthimaticExpression(Expression left, Expression right) : base(left, right)
        {

        }

        public abstract string Operation { get; }

        public override decimal Evaluate()
            => Operation switch
            {
                "+" => Left.Evaluate() + Right.Evaluate(),
                "-" => Left.Evaluate() - Right.Evaluate(),
                "*" => Left.Evaluate() * Right.Evaluate(),
                "/" => Left.Evaluate() / Right.Evaluate(),
                _ => throw new InvalidOperationException()
            };
    }
}
