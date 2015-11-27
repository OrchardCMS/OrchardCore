using Microsoft.Extensions.DependencyInjection;
using Orchard.Environment.Commands;
using System.IO;
using Xunit;

namespace Orchard.Tests.Commands
{
    public class CommandManagerTests
    {
        private ICommandManager _manager;

        public CommandManagerTests()
        {
            var services = new ServiceCollection();

            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, MyCommand>();

            _manager = services.BuildServiceProvider().GetService<ICommandManager>();
        }

        [Fact]
        public void ManagerCanRunACommand()
        {
            var context = new CommandParameters { Arguments = new string[] { "FooBar" }, Output = new StringWriter() };
            _manager.Execute(context);
            Assert.Equal("success!", context.Output.ToString());
        }

        [Fact]
        public void ManagerCanRunACompositeCommand()
        {
            var context = new CommandParameters { Arguments = ("Foo Bar Bleah").Split(' '), Output = new StringWriter() };
            _manager.Execute(context);
            Assert.Equal("Bleah", context.Output.ToString());
        }

        public class MyCommand : DefaultOrchardCommandHandler
        {
            public string FooBar()
            {
                return "success!";
            }

            public string Foo_Bar(string bleah)
            {
                return bleah;
            }
        }
    }
}