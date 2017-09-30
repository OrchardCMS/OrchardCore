using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Commands;
using System.IO;
using Xunit;
using Microsoft.Extensions.Localization;
using OrchardCore.Tests.Stubs;

namespace OrchardCore.Tests.Commands
{
    public class CommandManagerTests
    {
        private ICommandManager _manager;

        public CommandManagerTests()
        {
            var services = new ServiceCollection();

            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, MyCommand>();
            services.AddScoped<IStringLocalizer<DefaultCommandManager>, NullStringLocalizer<DefaultCommandManager>>();

            _manager = services.BuildServiceProvider().GetService<ICommandManager>();
        }

        [Fact]
        public void ManagerCanRunACommand()
        {
            var context = new CommandParameters { Arguments = new string[] { "FooBar" }, Output = new StringWriter() };
            _manager.ExecuteAsync(context);
            Assert.Equal("success!", context.Output.ToString());
        }

        [Fact]
        public void ManagerCanRunACompositeCommand()
        {
            var context = new CommandParameters { Arguments = ("Foo Bar Bleah").Split(' '), Output = new StringWriter() };
            _manager.ExecuteAsync(context);
            Assert.Equal("Bleah", context.Output.ToString());
        }

        public class MyCommand : DefaultCommandHandler
        {
            public MyCommand() : base(null) { }

            public string FooBar()
            {
                return "success!";
            }

            [CommandName("Foo Bar")]
            public string Foo_Bar(string bleah)
            {
                return bleah;
            }
        }
    }
}