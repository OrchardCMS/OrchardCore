using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OrchardCore.Admin.WebAssembly.App;
using OrchardCore.Common.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.RegisterCustomElement<OptionEditor>("option-editor");
builder.RootComponents.RegisterCustomElement<Initializer>("init");
builder.Services.AddScoped<ExampleJsInterop>();


await builder.Build().RunAsync();
