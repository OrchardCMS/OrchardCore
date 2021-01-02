using OrchardCore.Queries.Sql.Parser;
using Parlot.Fluent;
using Xunit;
using SqlParser = OrchardCore.Queries.Sql.Parser.SqlParser;

namespace OrchardCore.Tests.Modules.OrchardCore.Queries
{
    public class SqlParserTests
    {
        [Theory]
        [InlineData("12", 12)]
        [InlineData("12.5", 12.5)]
        public void ParseNumberExpression(string text, decimal expected)
        {
            // Arrange & Act
            SqlParser.Expression.TryParse(text, out var expression);

            // Assert
            Assert.Equal(expected, expression.EvaluateAsDecimal());
        }

        [Theory]
        [InlineData("TRUE", true)]
        [InlineData("trUE", true)]
        [InlineData("false", false)]
        [InlineData("False", false)]
        public void ParseBooleanExpression(string text, bool expected)
        {
            // Arrange & Act
            SqlParser.Expression.TryParse(text, out var expression);

            // Assert
            Assert.Equal(expected, expression.EvaluateAsBoolean());
        }

        [Theory]
        [InlineData("\"Hisham\"", "Hisham")]
        [InlineData("\'Hisham\'", "Hisham")]
        public void ParseStringExpression(string text, string expected)
        {
            // Arrange & Act
            SqlParser.Expression.TryParse(text, out var expression);

            // Assert
            Assert.Equal(expected, expression.Evaluate().ToString());
        }

        [Theory]
        [InlineData("Name", "Name")]
        [InlineData("Name123", "Name123")]
        [InlineData("_Name", "_Name")]
        [InlineData("First_Name", "First_Name")]
        public void ParseIdentifierExpression(string text, string expected)
        {
            // Arrange & Act
            SqlParser.Expression.TryParse(text, out var expression);

            // Assert
            Assert.Equal(expected, expression.Evaluate().ToString());
        }

        [Theory]
        [InlineData("5+2", 7)]
        [InlineData("5-2", 3)]
        [InlineData("5*2", 10)]
        [InlineData("5/2", 2.5)]
        [InlineData("12*3-6+70", 100)]
        [InlineData("150+50-5*5", 175)]
        public void ParseArithmaticExpression(string text, decimal expected)
        {
            // Arrange & Act
            SqlParser.Expression.TryParse(text, out var expression);

            // Assert
            Assert.Equal(expected, expression.EvaluateAsDecimal());
        }
    }
}
