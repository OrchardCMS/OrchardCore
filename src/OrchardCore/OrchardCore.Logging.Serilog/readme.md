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
        "Name": "File",
        "Args": {
          "path": "app_data/logs/orchard-log.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.ffff}|{TenantName}|{MachineName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Warning"
        }
      },
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

### Customize Serilog

All customization is done through the app settings file. For example, if you want to enrich
log with Machine Name and send logs to [seq](https://datalust.co/seq), you must reference
Serilog.Sinks.Seq and Serilog.Enrichers.Environment and make the following changes to appsettings.json

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
    "Enrich": [ "WithMachineName" ],
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
          "outputTemplate": "{Timestamp:HH:mm:ss}|{TenantName}|{MachineName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Information"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "app_data/logs/orchard-log.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.ffff}|{TenantName}|{MachineName}|{RequestId}|{SourceContext}|{Level:u3}|{Message:lj}{NewLine}{Exception}",
          "restrictedToMinimumLevel": "Warning"
        }
      },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seqserver:5341"
        }
      }
    ]
  },
```
