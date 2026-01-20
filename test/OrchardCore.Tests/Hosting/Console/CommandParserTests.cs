using OrchardCore.Environment.Commands.Parameters;

namespace OrchardCore.Tests.Hosting.Console
{
    public class CommandParserTests
    {
        [Fact]
        public void ParserUnderstandsSimpleArguments()
        {
            // a b cdef
            // => a
            // => b
            // => cdef
            var result = new CommandParser().Parse("a b cdef").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("a", result[0]);
            Assert.Equal("b", result[1]);
            Assert.Equal("cdef", result[2]);
        }

        [Fact]
        public void ParserIgnoresExtraSpaces()
        {
            //  a    b    cdef
            // => a
            // => b
            // => cdef
            var result = new CommandParser().Parse("  a    b    cdef   ").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("a", result[0]);
            Assert.Equal("b", result[1]);
            Assert.Equal("cdef", result[2]);
        }

        [Fact]
        public void ParserGroupsQuotedArguments()
        {
            // feature enable "a b cdef"
            // => feature
            // => enable
            // => a b cdef
            var result = new CommandParser().Parse("feature enable \"a b cdef\"").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("a b cdef", result[2]);
        }

        [Fact]
        public void ParserUnderstandsQuotesInsideArgument()
        {
            // feature enable /foo:"a b cdef"
            // => feature
            // => enable
            // => /foo:a b cdef
            var result = new CommandParser().Parse("feature enable /foo:\"a b cdef\"").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("/foo:a b cdef", result[2]);
        }

        [Fact]
        public void ParserBackslashEscapesQuote()
        {
            // feature enable \"a b cdef\"
            // => feature
            // => enable
            // => "a
            // => b
            // => cdef"
            var result = new CommandParser().Parse("feature enable \\\"a b cdef\\\"").ToList();
            Assert.NotNull(result);
            Assert.Equal(5, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("\"a", result[2]);
            Assert.Equal("b", result[3]);
            Assert.Equal("cdef\"", result[4]);
        }

        [Fact]
        public void ParserBackslashDoesnotEscapeBackslash()
        {
            // feature enable \\a
            // => feature
            // => enable
            // => \\a
            var result = new CommandParser().Parse("feature enable \\\\a").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("\\\\a", result[2]);
        }

        [Fact]
        public void ParserBackslashDoesnotEscapeOtherCharacters()
        {
            // feature enable \a
            // => feature
            // => enable
            // => \a
            var result = new CommandParser().Parse("feature enable \\a").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("\\a", result[2]);
        }

        [Fact]
        public void ParserUnderstandsTrailingBackslash()
        {
            // feature enable \
            // => feature
            // => enable
            // => \
            var result = new CommandParser().Parse("feature enable \\").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("\\", result[2]);
        }

        [Fact]
        public void ParserUnderstandsTrailingBackslash2()
        {
            // feature enable b\
            // => feature
            // => enable
            // => b\
            var result = new CommandParser().Parse("feature enable b\\").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("b\\", result[2]);
        }

        [Fact]
        public void ParserUnderstandsEmptyArgument()
        {
            // feature enable ""
            // => feature
            // => enable
            // => <empty arg>
            var result = new CommandParser().Parse("feature enable \"\"").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("", result[2]);
        }

        [Fact]
        public void ParserUnderstandsTrailingQuote()
        {
            // feature enable "
            // => feature
            // => enable
            // => <empty arg>
            var result = new CommandParser().Parse("feature enable \"").ToList();
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
            Assert.Equal("feature", result[0]);
            Assert.Equal("enable", result[1]);
            Assert.Equal("", result[2]);
        }

        [Fact]
        public void ParserUnderstandsEmptyArgument2()
        {
            // "
            // => <empty arg>
            var result = new CommandParser().Parse("\"").ToList();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("", result[0]);
        }
        [Fact]
        public void ParserUnderstandsEmptyArgument3()
        {
            // ""
            // => <empty arg>
            var result = new CommandParser().Parse("\"\"").ToList();
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("", result[0]);
        }
    }
}
