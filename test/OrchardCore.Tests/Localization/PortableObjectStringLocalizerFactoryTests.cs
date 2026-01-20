using OrchardCore.Localization;

namespace OrchardCore.Tests.Localization
{
    public class PortableObjectStringLocalizerFactoryTests
    {
        [Fact]
        public async Task LocalizerReturnsTranslationFromInnerClass()
            => await StartupRunner.Run(typeof(PortableObjectStringLocalizerFactory), "ar", "مرحبا");

        public class PortableObjectStringLocalizerFactory
        {
#pragma warning disable CA1822 // Mark members as static
            public void ConfigureServices(IServiceCollection services)
#pragma warning restore CA1822 // Mark members as static
            {
                services.AddMvc();
                services.AddLocalization();
                services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization/PoFiles");
                services.Replace(ServiceDescriptor.Singleton<ILocalizationFileLocationProvider, StubPoFileLocationProvider>());
            }

#pragma warning disable CA1822 // Mark members as static
            public void Configure(
#pragma warning restore CA1822 // Mark members as static
                IApplicationBuilder app,
                IStringLocalizer<Model> localizer)
            {
                var supportedCultures = new[] { "ar", "en" };
                app.UseRequestLocalization(options =>
                    options
                        .AddSupportedCultures(supportedCultures)
                        .AddSupportedUICultures(supportedCultures)
                        .SetDefaultCulture("ar")
                );

                app.Run(async (context) =>
                {
                    await context.Response.WriteAsync(localizer["Hello"]);
                });
            }
        }

        public class Model
        {
            public string Hello { get; set; }
        }
    }
}
