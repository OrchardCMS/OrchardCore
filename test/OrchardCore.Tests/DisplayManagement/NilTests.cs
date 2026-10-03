using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement;

public class NilTests
{
    [Fact]
    public void Nil_Default_EqualToNull()
    {
        var nil = Nil.Instance;

        Assert.True(nil == null);
        Assert.False(nil != null);

        Assert.True(nil == Nil.Instance);
        Assert.False(nil != Nil.Instance);
    }

    [Fact]
    public void Nil_Default_BeRecursive()
    {
        dynamic nil = Nil.Instance;

        Assert.True(nil == null);
        Assert.True(nil.Foo == null);
        Assert.True(nil.Foo.Bar == null);
    }

    [Fact]
    public void CallingToStringOnNil_Default_ReturnsEmpty()
    {
        var nil = Nil.Instance;
        Assert.Equal("", nil.ToString());
    }

    [Fact]
    public void CallingToStringOnDynamicNil_Default_ReturnsEmpty()
    {
        dynamic nil = Nil.Instance;
        Assert.Equal("", nil.Foo.Bar.ToString());
    }

    [Fact]
    public void ConvertingToString_Default_ReturnsNullString()
    {
        dynamic nil = Nil.Instance;
        Assert.True((string)nil == null);
    }
}
