namespace OrchardCore.Json.Nodes.Test;

public class Base64Tests
{
    [Theory]
    [InlineData("YTw+OmE/", "a<>:a?")]
    [InlineData("SGVsbA==", "Hell")]
    [InlineData("SGVsbG8=", "Hello")]
    [InlineData("", "")]
    public void MergeArrayShouldRespectJsonMergeSettings(string source, string expected)
    {
        Assert.Equal(expected, Base64.FromUTF8Base64String(source));
    }
}
