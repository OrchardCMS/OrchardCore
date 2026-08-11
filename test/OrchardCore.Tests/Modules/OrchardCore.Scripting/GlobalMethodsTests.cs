using OrchardCore.Scripting;
using OrchardCore.Scripting.Providers;

namespace OrchardCore.Tests.Modules.OrchardCore.Scripting;

public class GlobalMethodsTests
{
    [Fact]
    public void Base_64EncodeCompressedBase64_Succeeds()
    {
        var gzip = (Func<string, string>)CommonGeneratorMethods._gZip.Method.Invoke(null);
        var source = "H4sIAOCaLmcAA/NIzcnJVwjPL8pJUQQAoxwpHAwAAAA=";
        var expected = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World!"));

        Assert.Equal(expected, gzip(source));
    }

    [Fact]
    public void Base_64Decode_Succeeds()
    {
        var base64encode = (Func<string, string>)CommonGeneratorMethods._base64.Method.Invoke(null);
        var source = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World!"));
        var expected = "Hello World!";

        Assert.Equal(expected, base64encode(source));
    }

    [Fact]
    public void Html_Decode_Succeeds()
    {
        var htmldecode = (Func<string, string>)CommonGeneratorMethods._html.Method.Invoke(null);
        var source = "&lt;Hello&gt;";
        var expected = "<Hello>";

        Assert.Equal(expected, htmldecode(source));
    }

    [Fact]
    public void Encrypt_Decrypt_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        using var serviceProvider = services.BuildServiceProvider();

        var methods = new DataProtectionMethods();
        var methodDict = methods.GetMethods().ToDictionary(m => m.Name);

        var encrypt = (Func<string, string>)methodDict["encrypt"].Method(serviceProvider);
        var decrypt = (Func<string, string>)methodDict["decrypt"].Method(serviceProvider);

        const string plainText = "Hello World!";
        var encrypted = encrypt(plainText);
        var decrypted = decrypt(encrypted);

        Assert.NotEqual(plainText, encrypted);
        Assert.Equal(plainText, decrypted);
    }

    [Fact]
    public void Return_DecryptingEmptyInput_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        using var serviceProvider = services.BuildServiceProvider();

        var methods = new DataProtectionMethods();
        var methodDict = methods.GetMethods().ToDictionary(m => m.Name);

        var decrypt = (Func<string, string>)methodDict["decrypt"].Method(serviceProvider);

        Assert.Equal(string.Empty, decrypt(string.Empty));
    }

    [Fact]
    public void Return_DecryptingInvalidInput_Succeeds()
    {
        var services = new ServiceCollection();
        services.AddDataProtection();
        using var serviceProvider = services.BuildServiceProvider();

        var methods = new DataProtectionMethods();
        var methodDict = methods.GetMethods().ToDictionary(m => m.Name);

        var decrypt = (Func<string, string>)methodDict["decrypt"].Method(serviceProvider);

        Assert.Equal(string.Empty, decrypt(Convert.ToBase64String("not-a-valid-ciphertext"u8.ToArray())));
    }
}
