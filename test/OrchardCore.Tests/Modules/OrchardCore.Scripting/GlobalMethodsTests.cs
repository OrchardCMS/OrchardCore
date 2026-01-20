using OrchardCore.Scripting;

namespace OrchardCore.Json.Nodes.Test;

public class GlobalMethodsTests
{
    [Fact]
    public void ShouldBase64EncodeCompressedBase64()
    {
        var gzip = (Func<string, string>)CommonGeneratorMethods._gZip.Method.Invoke(null);
        var source = "H4sIAOCaLmcAA/NIzcnJVwjPL8pJUQQAoxwpHAwAAAA=";
        var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World!"));

        Assert.Equal(expected, gzip(source));
    }

    [Fact]
    public void ShouldBase64Decode()
    {
        var base64encode = (Func<string, string>)CommonGeneratorMethods._base64.Method.Invoke(null);
        var source = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World!"));
        var expected = "Hello World!";

        Assert.Equal(expected, base64encode(source));
    }

    [Fact]
    public void ShouldHtmlDecode()
    {
        var htmldecode = (Func<string, string>)CommonGeneratorMethods._html.Method.Invoke(null);
        var source = "&lt;Hello&gt;";
        var expected = "<Hello>";

        Assert.Equal(expected, htmldecode(source));
    }
}
