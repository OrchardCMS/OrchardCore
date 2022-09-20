var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrchardCore()
    .AddMvc();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

app.UseOrchardCore();

app.Run();
