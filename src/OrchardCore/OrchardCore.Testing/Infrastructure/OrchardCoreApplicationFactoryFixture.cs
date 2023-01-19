using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;
using OrchardCore.Testing.Apis;

namespace OrchardCore.Testing.Infrastructure;
//public class OrchardCoreApplicationFactoryFixture : WebApplicationFactory<Program>
//{
//    public OrchardCoreApplicationFactoryFixture()
//    {
//    }

//    protected override void ConfigureWebHost(IWebHostBuilder builder)
//    {
//        builder.UseStartup<SiteStartup>();
//        //builder.UseUrls("https://localhost:7048");

//        var shellsApplicationDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");

//        if (Directory.Exists(shellsApplicationDataPath))
//        {
//            Directory.Delete(shellsApplicationDataPath, true);
//        }

//        builder.UseContentRoot(Directory.GetCurrentDirectory());
//    }

//    //protected override IHost CreateHost(IHostBuilder builder)
//    //{
//    //    // need to create a plain host that we can return.
//    //    //var dummyHost = builder.Build();

//    //    // configure and start the actual host.
//    //    builder.ConfigureWebHost(webHostBuilder => webHostBuilder.UseKestrel());

//    //    var host = builder.Build();

//    //    host.Start();

//    //    return host;
//    //}

//    //protected override IWebHostBuilder CreateWebHostBuilder()
//    //    => WebHostBuilderFactory.CreateFromAssemblyEntryPoint(typeof(Program).Assembly, Array.Empty<string>());
//    //    //=> WebHostBuilderFactory.CreateFromAssemblyEntryPoint(SiteStartup.StartupClass.Assembly, Array.Empty<string>());

//    //protected override IHostBuilder CreateHostBuilder()
//    //    => Host
//    //        .CreateDefaultBuilder()
//    //        .ConfigureWebHostDefaults(b => b.UseStartup<SiteStartup>());
//}

