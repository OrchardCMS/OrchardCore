# Customizing encoding settings

By default ASP.NET MVC only allows ASCII char to be rendered without being encoded, either be it with the `HtmlEncoder`, `UrlEncoder` or `JavaScriptEncoder`.

An unfortunate result is that most internal chars will be encoded by default, resulting in bigger payload, even though this is technically correct.

## Disabling Unicode ranges encoding

ASP.NET MVC provides a way to configure these settings. Orchard Core uses the encoders registered in DI in order to respect any custom options. To disable Unicode chars encoding for all ranges, you can use this code in the `Configure()` method of the `Startup` class of your web application:

```csharp
services.Configure<WebEncoderOptions>(options => 
{
      options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All);
});
```

Ideally you would specify only the Unicode ranges that you will actually use.

## Using encoders in your code

It is recommended to use the encoders registered in the DI containers instead of using the static ones.

Don't call `HtmlEncode.Default` or any other `TextEncoder` but instead inject one in your service constructor like this:

```csharp
public class MyService
{
    private readonly HtmlEncoder _htmlEncoder;

    public MyService(HtmlEncoder htmlEncoder)
    {
        _htmlEncoder = htmlEncoder;
    }
}
```

This will ensure that the settings you have set for the Unicode ranges will also apply to your own code.