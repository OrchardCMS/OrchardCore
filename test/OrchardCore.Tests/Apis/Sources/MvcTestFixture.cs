// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tests.Apis;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class OrchardTestFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public string ShellsContainerName { get; set; } = "Sites_" + Guid.NewGuid().ToString().Replace("-", "");
        public string ShellsContainerPath => Path.Combine(Directory.GetCurrentDirectory(), "App_Data", ShellsContainerName);

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (Directory.Exists(ShellsContainerPath))
            {
                Directory.Delete(ShellsContainerPath, true);
            }

            builder.UseContentRoot(Directory.GetCurrentDirectory());

            builder
                .ConfigureServices(services => {
                    services.AddSingleton(new TestSiteConfiguration { ShellsContainerName = ShellsContainerName });
                });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHostBuilderFactory.CreateFromAssemblyEntryPoint(
                typeof(OrchardCore.Cms.Web.Startup).Assembly, Array.Empty<string>())
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<SiteStartup>();
        }
    }

    public class TestSiteConfiguration {
        public string ShellsContainerName { get; set; }
    }
}