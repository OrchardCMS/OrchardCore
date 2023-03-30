# OrchardCore.Logging.Serilog

`OrchardCore.Logging.Serilog` integrates [Serilog](https://serilog.net/) structured logging with OrchardCore.

## How to use

Add a reference to `OrchardCore.Logging.Serilog`.

### Add serilog configuration in appsettings.json

``` json
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Default": "Warning",
        "Microsoft": "Error",
        "System": "Error"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "{Timestamp:HH:mm:ss}|{TenantName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Information"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "App_Data/logs/orchard-log.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.ffff}|{TenantName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Warning"
        }
      }
    ]
  }
```

### Modify `program.cs` to use Serilog

``` csharp
        using Serilog;
        ...
        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => logging.ClearProviders())
                .UseSerilog((hostingContext, configBuilder) =>
                {
                    configBuilder.ReadFrom.Configuration(hostingContext.Configuration).Enrich.FromLogContext();
                })
                .ConfigureWebHostDefaults(webBuilder => webBuilder
                    .UseStartup<Startup>())
                .Build();
```

### Modify `startup.cs` to include TenantName in LogContext

``` csharp
        using OrchardCore.Logging;
        ...
        public void Configure(IApplicationBuilder app, IHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseOrchardCore(c => c.UseSerilogTenantNameLogging());
        }
```

## Credits

### Serilog

<https://github.com/serilog/serilog-aspnetcore>

Apache-2.0 License
