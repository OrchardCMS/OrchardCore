using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Sms.Services;

namespace OrchardCore.Sms;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<TwilioSmsService>();
        services.Configure<SmsServiceOptions>(options =>
        {
            options.SmsServices.Add("Twilio", (sp) => sp.GetService<TwilioSmsService>());
        });

        services.AddScoped(sp =>
        {
            var options = sp.GetService<IOptions<SmsServiceOptions>>().Value;

            if (options.SmsServices.TryGetValue(options.Name, out var service))
            {
                return service(sp);
            }

            return new ConsoleSmsService();
        });
    }
}
