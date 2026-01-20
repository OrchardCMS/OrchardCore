using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement
{
    public class CompositeTests
    {
        [Fact]
        public void CompositesShouldNotOverrideExistingMembers()
        {
            var composite = new Animal { Color = "Pink" };

            Assert.Equal("Pink", composite.Color);
        }

        [Fact]
        public void CompositesShouldNotOverrideExistingMembersWhenUsedAsDynamic()
        {
            dynamic composite = new Animal();

            composite.Color = "Pink";
            Assert.Equal("Pink", composite.Color);
        }

        [Fact]
        public void CompositesShouldAccessUnknownProperties()
        {
            dynamic composite = new Animal();

            composite.Fake = 42;
            Assert.Equal(42, composite.Fake);
        }

        [Fact]
        public void CompositesShouldAccessUnknownPropertiesByIndex()
        {
            dynamic composite = new Animal();

            composite["Fake"] = 42;
            Assert.Equal(42, composite["Fake"]);
        }

        [Fact]
        public void CompositesShouldAccessKnownPropertiesByIndex()
        {
            dynamic composite = new Animal();

            composite["Pink"] = "Pink";
            Assert.Equal("Pink", composite["Pink"]);
        }

        [Fact]
        public void ChainProperties()
        {
            dynamic foo = new Animal();
            foo.Bar("bar");

            Assert.Equal("bar", foo.Bar);
            Assert.False(foo.Bar == null);
        }
    }

    public class Animal : Composite
    {
        public string Kind { get; set; }
        public string Color { get; set; }
    }

    public interface ISized
    {
        int Size { get; set; }
    }
}
