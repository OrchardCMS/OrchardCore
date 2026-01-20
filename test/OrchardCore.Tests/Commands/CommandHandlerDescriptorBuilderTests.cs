using OrchardCore.Environment.Commands;

namespace OrchardCore.Tests.Commands
{
    public class CommandHandlerDescriptorBuilderTests
    {
        [Fact]
        public void BuilderShouldCreateDescriptor()
        {
            var builder = new CommandHandlerDescriptorBuilder();
            var descriptor = builder.Build(typeof(MyCommand));
            Assert.NotNull(descriptor);
            Assert.Equal(4, descriptor.Commands.Count());
            Assert.NotNull(descriptor.Commands.SingleOrDefault(d => d.Names.Contains("FooBar")));
            Assert.Equal(typeof(MyCommand).GetMethod("FooBar"), descriptor.Commands.Single(d => d.Names.Contains("FooBar")).MethodInfo);
            Assert.NotNull(descriptor.Commands.SingleOrDefault(d => d.Names.Contains("MyCommand")));
            Assert.Equal(typeof(MyCommand).GetMethod("FooBar2"), descriptor.Commands.Single(d => d.Names.Contains("MyCommand")).MethodInfo);
            Assert.NotNull(descriptor.Commands.SingleOrDefault(d => d.Names.Contains("Foo Bar")));
            Assert.Equal(typeof(MyCommand).GetMethod("Foo_Bar"), descriptor.Commands.Single(d => d.Names.Contains("Foo Bar")).MethodInfo);
            Assert.NotNull(descriptor.Commands.SingleOrDefault(d => d.Names.Contains("Foo_Bar")));
            Assert.Equal(typeof(MyCommand).GetMethod("Foo_Bar3"), descriptor.Commands.Single(d => d.Names.Contains("Foo_Bar")).MethodInfo);
        }

        public class MyCommand : DefaultCommandHandler
        {
            public MyCommand() : base(null)
            {
            }

#pragma warning disable CA1822 // Mark members as static
            public void FooBar()
            {
            }

            [CommandName("MyCommand")]
            public void FooBar2()
            {
            }

            [CommandName("Foo Bar")]
            public void Foo_Bar()
            {
            }

            [CommandName("Foo_Bar")]
            public void Foo_Bar3()
            {
            }
#pragma warning restore CA1822 // Mark members as static
        }

        [Fact]
        public void BuilderShouldReturnPublicMethodsOnly()
        {
            var builder = new CommandHandlerDescriptorBuilder();
            var descriptor = builder.Build(typeof(PublicMethodsOnly));
            Assert.NotNull(descriptor);
            Assert.Single(descriptor.Commands);
            Assert.NotNull(descriptor.Commands.SingleOrDefault(d => d.Names.Contains("Method")));
        }

#pragma warning disable 660,661
        public class PublicMethodsOnly
        {
#pragma warning restore 660,661
            public bool Bar { get; set; }   // No accessors.
            public bool Field = true;       // No field.

            // No private method.
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable IDE0051 // Remove unused private members
            private void Blah()
#pragma warning restore IDE0051 // Remove unused private members
#pragma warning restore CA1822 // Mark members as static
            {
            }

            // No private method.
            public static void Foo()
            {
            }

            // No operator.
            public static bool operator ==(PublicMethodsOnly _1, PublicMethodsOnly _2)
            {
                return false;
            }

            public static bool operator !=(PublicMethodsOnly _1, PublicMethodsOnly _2)
            {
                return false;
            }

#pragma warning disable CA1822 // Mark members as static
            public void Method()
#pragma warning restore CA1822 // Mark members as static
            {
            }
        }
    }
}
