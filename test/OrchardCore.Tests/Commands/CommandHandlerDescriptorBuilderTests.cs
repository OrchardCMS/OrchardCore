using System.Linq;
using OrchardCore.Environment.Commands;
using Xunit;

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
            public bool Bar { get; set; }   // no accessors
            public bool Field = true;       // no field

            // no private method
            private void Blah()
            {
            }

            // no private method
            public static void Foo()
            {
            }

            // no operator
            public static bool operator ==(PublicMethodsOnly a, PublicMethodsOnly b)
            {
                return false;
            }

            public static bool operator !=(PublicMethodsOnly a, PublicMethodsOnly b)
            {
                return false;
            }

            public void Method()
            {
            }
        }
    }
}
