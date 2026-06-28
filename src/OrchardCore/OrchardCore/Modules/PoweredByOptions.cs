namespace OrchardCore.Modules;

public class PoweredByOptions
{
    private const string PoweredByHeaderName = "X-Powered-By";
    private const string PoweredByHeaderValue = "OrchardCore";

    public string HeaderName { get; set; } = PoweredByHeaderName;

    public string HeaderValue { get; set; } = PoweredByHeaderValue;
}
