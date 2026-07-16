using OrchardCore.Apis.GraphQL.Client;

namespace OrchardCore.Tests.Apis.GraphQL.Client;

public class ContentTypeQueryResourceBuilderTests
{
    [Theory]
    [InlineData("someContentType")]
    [InlineData("SomeContentType")]
    public void Return_ContentTypeNameAsPascalCase_Succeeds(string contentType)
    {
        var result = new ContentTypeQueryResourceBuilder(contentType)
                        .Build();

        Assert.Equal("someContentType {}", result);
    }

    [Fact]
    public void Be_AbleToAddArgument_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithQueryArgument("arg", "1");

        Assert.Equal("someContentType(arg: 1) {}", builder.Build());
    }

    [Fact]
    public void Be_AbleToAddMultipleArguments_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithQueryArgument("arg", "1");
        builder.WithQueryArgument("arg2", "2");

        Assert.Equal("someContentType(arg: 1 arg2: 2) {}", builder.Build());
    }

    [Fact]
    public void Be_AbleToAddStringArgument_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithQueryStringArgument("arg", "a string");

        Assert.Equal("someContentType(arg: \"a string\") {}", builder.Build());
    }

    [Fact]
    public void Be_AbleToAddMultiInputArgument_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithQueryArgument("arg", "a", "1");
        builder.WithQueryArgument("arg", "b", "2");

        Assert.Equal("someContentType(arg:{a: 1, b: 2}) {}", builder.Build());
    }

    [Fact]
    public void Not_BeAbleToAddDuplicateArguments_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithQueryArgument("arg", "a");

        Assert.Throws<Exception>(() => builder.WithQueryArgument("arg", "b"));
        Assert.Throws<Exception>(() => builder.WithQueryArgument("arg", "b", "2"));
        Assert.Throws<Exception>(() => builder.WithNestedQueryArgument("arg", "b", "2"));
    }

    [Theory]
    [InlineData("someField")]
    [InlineData("SomeField")]
    public void Add_FieldWithNameAsPascalCase_Succeeds(string fieldName)
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithField(fieldName);

        Assert.Equal("someContentType { someField }", builder.Build());
    }

    [Theory]
    [InlineData("someNestedField")]
    [InlineData("SomeNestedField")]
    public void Add_NestedFieldWithNameAsPascalCase_Succeeds(string fieldName)
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithNestedField(fieldName);

        Assert.Equal("someContentType { someNestedField {} }", builder.Build());
    }

    [Fact]
    public void Be_AbleToAddMultipleNestedFields_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithField("field");
        builder.WithNestedField("nestedField").WithField("nestedChild");
        builder.WithNestedField("nestedField2").WithField("nestedChild2");

        Assert.Equal("someContentType { field nestedField { nestedChild } nestedField2 { nestedChild2 } }", builder.Build());
    }

    [Fact]
    public void Be_AbleToCombineNonNestedAndNestedFields_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithField("field");
        builder.WithNestedField("nestedField").WithField("nestedChild");

        Assert.Equal("someContentType { field nestedField { nestedChild } }", builder.Build());
    }

    [Fact]
    public void Be_AbleToAddNestedFieldsMultipleLevelsDeep_Succeeds()
    {
        var builder = new ContentTypeQueryResourceBuilder("someContentType");
        builder.WithField("field");
        builder.WithNestedField("nestedField").WithNestedField("nestednestedField").WithField("nestedChild");

        Assert.Equal("someContentType { field nestedField { nestednestedField { nestedChild } } }", builder.Build());
    }
}
