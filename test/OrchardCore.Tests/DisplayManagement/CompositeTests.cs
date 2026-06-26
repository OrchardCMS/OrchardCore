using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement;

public class CompositeTests
{
    [Fact]
    public void Composites_Default_NotOverrideExistingMembers()
    {
        var composite = new Animal { Color = "Pink" };

        Assert.Equal("Pink", composite.Color);
    }

    [Fact]
    public void Composites_UsedAsDynamic_NotOverrideExistingMembers()
    {
        dynamic composite = new Animal();

        composite.Color = "Pink";
        Assert.Equal("Pink", composite.Color);
    }

    [Fact]
    public void Composites_Default_AccessUnknownProperties()
    {
        dynamic composite = new Animal();

        composite.Fake = 42;
        Assert.Equal(42, composite.Fake);
    }

    [Fact]
    public void Composites_Default_AccessUnknownPropertiesByIndex()
    {
        dynamic composite = new Animal();

        composite["Fake"] = 42;
        Assert.Equal(42, composite["Fake"]);
    }

    [Fact]
    public void Composites_Default_AccessKnownPropertiesByIndex()
    {
        dynamic composite = new Animal();

        composite["Pink"] = "Pink";
        Assert.Equal("Pink", composite["Pink"]);
    }

    [Fact]
    public void ChainProperties_Default_Succeeds()
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
