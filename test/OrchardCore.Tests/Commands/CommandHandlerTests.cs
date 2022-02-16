using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Environment.Commands;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Commands
{
    public class CommandsTests
    {
        private ICommandHandler _handler;

        public CommandsTests()
        {
            _handler = new StubCommandHandler();
        }

        private CommandContext CreateCommandContext(string commandName)
        {
            return CreateCommandContext(commandName, new Dictionary<string, string>(), new string[] { });
        }

        private CommandContext CreateCommandContext(string commandName, IDictionary<string, string> switches)
        {
            return CreateCommandContext(commandName, switches, new string[] { });
        }

        private CommandContext CreateCommandContext(string commandName, IDictionary<string, string> switches, string[] args)
        {
            var builder = new CommandHandlerDescriptorBuilder();

            var descriptor = builder.Build(typeof(StubCommandHandler));

            var commandDescriptor = descriptor.Commands.Single(d => d.Names.Any(x => string.Equals(x, commandName, StringComparison.OrdinalIgnoreCase)));

            return new CommandContext
            {
                Command = commandName,
                Switches = switches,
                CommandDescriptor = commandDescriptor,
                Arguments = args,
                Input = new StringReader(string.Empty),
                Output = new StringWriter()
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
            string trace = "Command Baz Called";

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
            string concatenated = "";
            foreach (var s in parameters)
            {
                concatenated += s;
            }
            return concatenated;
        }

        public string ConcatAllParams(string leftmost, params string[] rest)
        {
            string concatenated = leftmost;
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
    }
}
