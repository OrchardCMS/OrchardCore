using System;
using System.Globalization;
using System.Linq.Expressions;
using OrchardCore.Modules;
using Parlot;
using Parlot.Fluent;
using static Parlot.Fluent.Parsers;

namespace OrchardCore.AuditTrail.Services
{
    public readonly struct BuildExpressionContext
    {
        public BuildExpressionContext(DateTime utcNow, ParameterExpression parameter, MemberExpression member, Type type)
        {
            UtcNow = utcNow;
            Parameter = parameter;
            Member = member;
            Type = type;
        }

        public DateTime UtcNow { get; }
        public ParameterExpression Parameter { get; }
        public MemberExpression Member { get; }
        public Type Type { get; }
    }

    public abstract class Node
    {
        public abstract Expression BuildExpression(in BuildExpressionContext context);
    }

    public abstract class OperatorNode : Node
    {
        public string Operator { get; set; }

        public Expression BuildOperation(in BuildExpressionContext context, ConstantExpression constant)
        {
            if (String.IsNullOrEmpty(Operator))
            {
                return constant;
            }

            return Operator switch
            {
                ">" => Expression.GreaterThan(context.Member, constant),
                ">=" => Expression.GreaterThanOrEqual(context.Member, constant),
                "<" => Expression.LessThan(context.Member, constant),
                "<=" => Expression.LessThanOrEqual(context.Member, constant),
                _ => null
            };
        }
    }

    public class DateNode : OperatorNode
    {
        public DateNode(DateTimeOffset dateTime)
        {
            DateTime = dateTime;
        }

        public DateTimeOffset DateTime { get; }

        public override Expression BuildExpression(in BuildExpressionContext context)
            => BuildOperation(context, Expression.Constant(DateTime.UtcDateTime, typeof(DateTime)));

        public override string ToString()
            => $"{(String.IsNullOrEmpty(Operator) ? String.Empty : Operator)}{DateTime.ToString("o")}";
    }

    public class DateNode2 : OperatorNode
    {
        public DateNode2(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        public DateTime DateTime { get; }

        public override Expression BuildExpression(in BuildExpressionContext context)
            => BuildOperation(context, Expression.Constant(DateTime, typeof(DateTime)));

        public override string ToString()
            => $"{(String.IsNullOrEmpty(Operator) ? String.Empty : Operator)}{DateTime.ToString("o")}";
    }

    public class NowNode : OperatorNode
    {
        public NowNode()
        {
        }
        public NowNode(long arithmetic)
        {
            Arithmetic = arithmetic;
        }

        public long? Arithmetic { get; }

        public override Expression BuildExpression(in BuildExpressionContext context)
            => BuildOperation(context, Expression.Constant(context.UtcNow.AddDays(Arithmetic.GetValueOrDefault()), typeof(DateTime)));

        public override string ToString()
            => $"{(String.IsNullOrEmpty(Operator) ? String.Empty : Operator)}@now{(Arithmetic.HasValue ? Arithmetic.Value.ToString() : String.Empty)}";
    }

    public class TodayNode : NowNode
    {
        public TodayNode()
        {
        }

        public TodayNode(long arithmetic) : base(arithmetic)
        {
        }

        public override string ToString()
            => $"{(String.IsNullOrEmpty(Operator) ? String.Empty : Operator)}@today{(Arithmetic.HasValue ? Arithmetic.Value.ToString() : String.Empty)}";

    }

    public abstract class ExpressionNode : Node
    { }

    public class UnaryExpressionNode : ExpressionNode
    {
        public UnaryExpressionNode(OperatorNode node)
        {
            Node = node;
        }

        public override Expression BuildExpression(in BuildExpressionContext context)
            => Expression.Lambda(context.Type, Node.BuildExpression(context), context.Parameter);

        public OperatorNode Node { get; }
        public override string ToString()
            => Node.ToString();
    }

    public class BinaryExpressionNode : ExpressionNode
    {
        public BinaryExpressionNode(OperatorNode left, OperatorNode right)
        {
            Left = left;
            Right = right;
        }

        public OperatorNode Left { get; }
        public OperatorNode Right { get; }

        public override Expression BuildExpression(in BuildExpressionContext context)
        {
            var left = Expression.GreaterThanOrEqual(context.Member, Left.BuildExpression(context));
            var right = Expression.LessThanOrEqual(context.Member, Right.BuildExpression(context));

            return Expression.Lambda(context.Type, Expression.AndAlso(left, right), context.Parameter);
        }

        public override string ToString()
            => $"{Left.ToString()}..{Right.ToString()}";
    }

    public static class DateTimeParser
    {
        public static Parser<ExpressionNode> Parser;

        static DateTimeParser()
        {
            var operators = OneOf(Literals.Text(">"), Literals.Text(">="), Literals.Text("<"), Literals.Text("<="));

            var arithmetic = Terms.Integer(NumberOptions.AllowSign);
            var range = Literals.Text("..");

            var todayParser = Terms.Text("@today").And(ZeroOrOne(arithmetic))
                 .Then<OperatorNode>(x =>
                 {
                     if (x.Item2 != 0)
                     {
                         return new TodayNode(x.Item2);
                     }

                     return new TodayNode();
                 });

            var nowParser = Terms.Text("@now").And(ZeroOrOne(arithmetic))
                .Then<OperatorNode>(x =>
                {
                    if (x.Item2 != 0)
                    {
                        return new NowNode(x.Item2);
                    }

                    return new NowNode();
                });

            var dateParser = AnyCharBefore(range)
                .Then<OperatorNode>((ctx, x) =>
                {
                    var context = (DateTimeParseContext)ctx;
                    var dateValue = x.ToString();

                    // Try o, primarily for times, and fall back to local timezone                    
                    if (DateTimeOffset.TryParseExact(dateValue, "yyyy-MM-dd'T'HH:mm:ss.FFFK", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTimeOffset))
                    {
                        return new DateNode2(dateTimeOffset.UtcDateTime);
                    }
                    else
                    {
                        var success = true;
                        if (!DateTime.TryParse(dateValue, context.CultureInfo, DateTimeStyles.None, out var dateTime))
                        {
                            if (!DateTime.TryParse(dateValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
                            {
                                success = false;
                            }
                        }

                        // If no timezone is specified, assume local using the configured timezone
                        if (success)
                        {
                            var converted = context.Clock.ConvertToTimeZone(dateTime, context.UserTimeZone);
                            return new DateNode2(converted.UtcDateTime.Date);
                        }
                    }

                    throw new ParseException("Could not parse date", context.Scanner.Cursor.Position);
                });

            var currentParser = OneOf(nowParser, todayParser);

            var valueParser = OneOf(currentParser, dateParser);

            var rangeParser = valueParser
                .And(ZeroOrOne(range.SkipAnd(OneOf(currentParser, dateParser))))
                .Then<ExpressionNode>(x =>
                {
                    if (x.Item2 == null)
                    {
                        return new UnaryExpressionNode(x.Item1);
                    }

                    else
                    {
                        return new BinaryExpressionNode(x.Item1, x.Item2);
                    }
                });

            Parser = operators.And(valueParser)
                    .Then<ExpressionNode>(x =>
                    {
                        x.Item2.Operator = x.Item1;
                        return new UnaryExpressionNode(x.Item2);
                    })
                .Or(rangeParser).Compile();
        }
    }

    public class DateTimeParseContext : ParseContext
    {
        public DateTimeParseContext(CultureInfo cultureInfo, IClock clock, ITimeZone userTimeZone, Scanner scanner, bool useNewLines = false) : base(scanner, useNewLines)
        {
            CultureInfo = cultureInfo;
            Clock = clock;
            UserTimeZone = userTimeZone;
        }

        public CultureInfo CultureInfo { get; set; }
        public IClock Clock { get; set; }
        public ITimeZone UserTimeZone { get; set; }
    }
}
