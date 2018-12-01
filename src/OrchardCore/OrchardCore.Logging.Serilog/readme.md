# OrchardCore.Logging.Serilog

`OrchardCore.Logging.Serilog` integrates [Serilog](https://serilog.net/) structured logging with OrchardCore

## How to use

add a reference to `OrchardCore.Logging.Serilog`

### add serilog configuration in appsettings.json

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
        "Name": "RollingFile",
        "Args": {
          "pathFormat": "app_data/logs/orchard-log-{Date}.txt",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.ffff}|{TenantName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Warning"
        }
      }
    ]
  }
```

### Modify `program.cs` to use Serilog

``` csharp
        public static IWebHost BuildWebHost(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                .UseSerilogWeb()
                .UseStartup<Startup>()
                .Build();
```

### Modify `startup.cs` to include TenantName in LogContext

``` csharp
        using OrchardCore.Logging;

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseStaticFiles();
            app.UseOrchardCore(c => c.UseSerilogTenantNameLoggingMiddleware());
        }
```