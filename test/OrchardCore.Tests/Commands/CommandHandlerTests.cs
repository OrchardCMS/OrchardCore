using OrchardCore.Environment.Commands;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Commands
{
    public class CommandsTests
    {
        private readonly ICommandHandler _handler;

        public CommandsTests()
        {
            _handler = new StubCommandHandler();
        }

        private static CommandContext CreateCommandContext(string commandName)
        {
            return CreateCommandContext(commandName, new Dictionary<string, string>(), Array.Empty<string>());
        }

        private static CommandContext CreateCommandContext(string commandName, IDictionary<string, string> switches)
        {
            return CreateCommandContext(commandName, switches, Array.Empty<string>());
        }

        private static CommandContext CreateCommandContext(string commandName, IDictionary<string, string> switches, string[] args)
        {
            var builder = new CommandHandlerDescriptorBuilder();

            var descriptor = builder.Build(typeof(StubCommandHandler));

            var commandDescriptor = descriptor.Commands.Single(d => d.Names.Any(x => String.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)));

            return new CommandContext
            {
                Command = commandName,
                Switches = switches,
                CommandDescriptor = commandDescriptor,
                Arguments = args,
                Input = new StringReader(String.Empty),
                Output = new StringWriter(),
            };
        }

        [Fact]
        public async Task TestFooCommand()
        {
            var commandContext = CreateCommandContext("Foo");
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("Command Foo Executed", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestNotExistingCommand()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                var commandContext = CreateCommandContext("NoSuchCommand");
                await _handler.ExecuteAsync(commandContext);
            });
        }

        [Fact]
        public async Task TestCommandWithCustomAlias()
        {
            var commandContext = CreateCommandContext("Bar");
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("Hello World!", commandContext.Output.ToString());
        }

        [Fact]
        public void TestHelpText()
        {
            var commandContext = CreateCommandContext("Baz");
            Assert.Equal("Baz help", commandContext.CommandDescriptor.HelpText);
        }

        [Fact]
        public void TestEmptyHelpText()
        {
            var commandContext = CreateCommandContext("Foo");
            Assert.Empty(commandContext.CommandDescriptor.HelpText);
        }

        [Fact]
        public async Task TestCaseInsensitiveForCommand()
        {
            var commandContext = CreateCommandContext("BAZ", new Dictionary<string, string> { { "VERBOSE", "true" } });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("Command Baz Called : This was a test", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestBooleanSwitchForCommand()
        {
            var commandContext = CreateCommandContext("Baz", new Dictionary<string, string> { { "Verbose", "true" } });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("Command Baz Called : This was a test", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestIntSwitchForCommand()
        {
            var commandContext = CreateCommandContext("Baz", new Dictionary<string, string> { { "Level", "2" } });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("Command Baz Called : Entering Level 2", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestStringSwitchForCommand()
        {
            var commandContext = CreateCommandContext("Baz", new Dictionary<string, string> { { "User", "OrchardUser" } });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("Command Baz Called : current user is OrchardUser", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestSwitchForCommandWithoutSupportForIt()
        {
            var switches = new Dictionary<string, string> { { "User", "OrchardUser" } };
            var commandContext = CreateCommandContext("Foo", switches);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.ExecuteAsync(commandContext));
        }

        [Fact]
        public async Task TestCommandThatDoesNotReturnAValue()
        {
            var commandContext = CreateCommandContext("Log");
            await _handler.ExecuteAsync(commandContext);
            Assert.Empty(commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestNotExistingSwitch()
        {
            var switches = new Dictionary<string, string> { { "ThisSwitchDoesNotExist", "Insignificant" } };
            var commandContext = CreateCommandContext("Foo", switches);
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.ExecuteAsync(commandContext));
        }

        [Fact]
        public async Task TestCommandArgumentsArePassedCorrectly()
        {
            var commandContext = CreateCommandContext("Concat", new Dictionary<string, string>(), new[] { "left to ", "right" });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("left to right", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestCommandArgumentsArePassedCorrectlyWithAParamsParameters()
        {
            var commandContext = CreateCommandContext("ConcatParams", new Dictionary<string, string>(), new[] { "left to ", "right" });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("left to right", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestCommandArgumentsArePassedCorrectlyWithAParamsParameterAndNoArguments()
        {
            var commandContext = CreateCommandContext("ConcatParams", new Dictionary<string, string>());
            await _handler.ExecuteAsync(commandContext);
            Assert.Empty(commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestCommandArgumentsArePassedCorrectlyWithNormalParametersAndAParamsParameters()
        {
            var commandContext = CreateCommandContext("ConcatAllParams",
                new Dictionary<string, string>(),
                new[] { "left-", "center-", "right" });
            await _handler.ExecuteAsync(commandContext);
            Assert.Equal("left-center-right", commandContext.Output.ToString());
        }

        [Fact]
        public async Task TestCommandParamsMismatchWithoutParamsNotEnoughArguments()
        {
            var commandContext = CreateCommandContext("Concat", new Dictionary<string, string>(), new[] { "left to " });
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.ExecuteAsync(commandContext));
        }

        [Fact]
        public async Task TestCommandParamsMismatchWithoutParamsTooManyArguments()
        {
            var commandContext = CreateCommandContext("Foo", new Dictionary<string, string>(), new[] { "left to " });
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.ExecuteAsync(commandContext));
        }

        [Fact]
        public async Task TestCommandParamsMismatchWithParamsButNotEnoughArguments()
        {
            var commandContext = CreateCommandContext("ConcatAllParams", new Dictionary<string, string>());
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _handler.ExecuteAsync(commandContext));
        }
    }

    public class StubCommandHandler : DefaultCommandHandler
    {
        public StubCommandHandler() : base(new NullStringLocalizerFactory().Create(typeof(object)))
        {
        }

        [OrchardSwitch]
        public bool Verbose { get; set; }

        [OrchardSwitch]
        public int Level { get; set; }

        [OrchardSwitch]
        public string User { get; set; }

#pragma warning disable CA1822 // Mark members as static
        public string Foo()
        {
            return "Command Foo Executed";
        }

        [CommandName("Bar")]
        public string Hello()
        {
            return "Hello World!";
        }

        [OrchardSwitches("Verbose, Level, User")]
        [CommandHelp("Baz help")]
        public string Baz()
        {
            var trace = "Command Baz Called";

            if (Verbose)
            {
                trace += " : This was a test";
            }

            if (Level == 2)
            {
                trace += " : Entering Level 2";
            }

            if (!String.IsNullOrEmpty(User))
            {
                trace += " : current user is " + User;
            }

            return trace;
        }

        public string Concat(string left, string right)
        {
            return left + right;
        }

        public string ConcatParams(params string[] parameters)
        {
            var concatenated = "";
            foreach (var s in parameters)
            {
                concatenated += s;
            }
            return concatenated;
        }

        public string ConcatAllParams(string leftmost, params string[] rest)
        {
            var concatenated = leftmost;
            foreach (var s in rest)
            {
                concatenated += s;
            }
            return concatenated;
        }

        public void Log()
        {
            return;
        }
#pragma warning restore CA1822 // Mark members as static
    }
}
