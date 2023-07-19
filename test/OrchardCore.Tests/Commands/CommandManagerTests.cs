using OrchardCore.Environment.Commands;
using OrchardCore.Localization;

namespace OrchardCore.Tests.Commands
{
    public class CommandManagerTests
    {
        private readonly ICommandManager _manager;

        public CommandManagerTests()
        {
            var services = new ServiceCollection();

            services.AddScoped<ICommandManager, DefaultCommandManager>();
            services.AddScoped<ICommandHandler, MyCommand>();
            services.AddSingleton<IStringLocalizerFactory, NullStringLocalizerFactory>();
            services.AddTransient(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            _manager = services.BuildServiceProvider().GetService<ICommandManager>();
        }

        [Fact]
        public async Task ManagerCanRunACommand()
        {
            var context = new CommandParameters { Arguments = new string[] { "FooBar" }, Output = new StringWriter() };
            await _manager.ExecuteAsync(context);
            Assert.Equal("success!", context.Output.ToString());
        }

        [Fact]
        public async Task ManagerCanRunACompositeCommand()
        {
            var context = new CommandParameters { Arguments = ("Foo Bar Bleah").Split(' '), Output = new StringWriter() };
            await _manager.ExecuteAsync(context);
            Assert.Equal("Bleah", context.Output.ToString());
        }

        public class MyCommand : DefaultCommandHandler
        {
            public MyCommand() : base(null)
            {
            }

#pragma warning disable CA1822 // Mark members as static
            public string FooBar()
            {
                return "success!";
            }

            [CommandName("Foo Bar")]
            public string Foo_Bar(string bleah)
            {
                return bleah;
            }
#pragma warning restore CA1822 // Mark members as static
        }
    }
}
