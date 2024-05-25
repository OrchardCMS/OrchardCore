using Microsoft.Extensions.Localization;

namespace OrchardCore.Demo.Services;
public class TestLocalizationService
{
    private readonly IStringLocalizer S;

    public TestLocalizationService(IStringLocalizer<TestLocalizationService> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public string SayHello()
    {
        return S["This is an internationalisation rule test."];
    }
}
