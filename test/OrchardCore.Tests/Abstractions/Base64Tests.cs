namespace OrchardCore.Json.Nodes.Test;

public class Base64Tests
{
    [Theory]
    [InlineData("YTw+OmE/", "a<>:a?")]
    [InlineData("SGVsbA==", "Hell")]
    [InlineData("SGVsbG8=", "Hello")]
    [InlineData("", "")]
    public void DecodeToStringTest(string source, string expected)
    {
        Assert.Equal(expected, Base64.FromUTF8Base64String(source));
    }

    [Theory]
    [InlineData("YTw+OmE/", "a<>:a?")]
    [InlineData("SGVsbA==", "Hell")]
    [InlineData("SGVsbG8=", "Hello")]
    [InlineData("", "")]
    public void DecodeToStreamTest(string source, string expected)
    {
        using var stream = Base64.DecodeToStream(source);
        using var sr = new StreamReader(stream);
        {
            Assert.Equal(expected, sr.ReadToEnd());
        }
    }
}
