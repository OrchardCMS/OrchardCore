using OrchardCore.Search.AzureAI;

namespace OrchardCore.Tests.Modules.OrchardCore.Search.AzureAI;

public class NamingTests
{
    private static readonly string _maxName = new('a', 128);
    private static readonly string _overMaxLength = new('a', 129);

    [Fact]
    public void CreateLengthSafeIndexName()
    {


        var isValid1 = AzureAISearchIndexNamingHelper.TryGetSafeIndexName(_maxName, out var result1);

        Assert.True(isValid1);

        Assert.Equal(_maxName, result1);

        var isValid2 = AzureAISearchIndexNamingHelper.TryGetSafeIndexName(_overMaxLength, out var result2);

        Assert.True(isValid2);

        Assert.Equal(_maxName, result2);
    }

    [Theory]
    [InlineData("IndexName_1", "indexname1")]
    [InlineData("Index-Name-1", "index-name-1")]
    [InlineData("123Index-Name-1", "123index-name-1")]
    [InlineData("_index-Name-1", "index-name-1")]
    [InlineData("123_-", "123-")]
    [InlineData("1234t", "1234t")]
    [InlineData("a", null)]
    [InlineData("___%^&", null)]
    public void CreateSafeIndexName(string indexName, string expectedName)
    {
        var valid = AzureAISearchIndexNamingHelper.TryGetSafeIndexName(indexName, out var result);

        if (expectedName != null)
        {
            Assert.True(valid);
        }
        else
        {
            Assert.False(valid);
        }

        Assert.Equal(expectedName, result);
    }

    [Fact]
    public void CreateLengthSafeFieldName()
    {
        var isValid1 = AzureAISearchIndexNamingHelper.TryGetSafeFieldName(_maxName, out var result1);

        Assert.True(isValid1);

        Assert.Equal(_maxName, result1);

        var isValid2 = AzureAISearchIndexNamingHelper.TryGetSafeFieldName(_overMaxLength, out var result2);

        Assert.True(isValid2);

        Assert.Equal(_maxName, result2);
    }

    [Theory]
    [InlineData("FieldName_1", "FieldName_1")]
    [InlineData("Field-Name-1", "FieldName1")]
    [InlineData("123Field-Name-1", "FieldName1")]
    [InlineData("_field-Name-1", "fieldName1")]
    [InlineData("a__B", "a__B")]
    [InlineData("a.b", "a__b")]
    [InlineData("a.b.c.d", "a__b__c__d")]
    [InlineData("a", "a")]
    [InlineData("1", null)]
    [InlineData("-", null)]
    [InlineData("a1", "a1")]
    [InlineData("azureSearchFieldName", "FieldName")]
    [InlineData("azureSearch_FieldName", "FieldName")]
    public void CreateSafeFieldName(string indexName, string expectedName)
    {
        var valid = AzureAISearchIndexNamingHelper.TryGetSafeFieldName(indexName, out var result);

        if (expectedName != null)
        {
            Assert.True(valid);
        }
        else
        {
            Assert.False(valid);
        }

        Assert.Equal(expectedName, result);
    }
}
