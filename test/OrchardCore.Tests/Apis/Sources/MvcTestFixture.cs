// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tests.Apis;
using OrchardCore.Tests.Apis.Sources;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class OrchardTestFixture<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        public string SiteName => "Sites_" + Guid.NewGuid().ToString().Replace("-", "");
        public string AppData => Path.Combine(EnvironmentHelpers.GetApplicationPath(), "App_Data", SiteName);

        public string Root => EnvironmentHelpers.GetApplicationPath();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            if (Directory.Exists(AppData))
            {
                Directory.Delete(AppData, true);
            }

            builder.UseContentRoot(Root);

            builder
                .ConfigureServices(services => {
                    services.AddSingleton(new TestSiteConfiguration { SiteName = SiteName });
                });
        }

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHostBuilderFactory.CreateFromAssemblyEntryPoint(
                typeof(OrchardCore.Cms.Web.Startup).Assembly, Array.Empty<string>())
                .UseContentRoot(Root)
                .UseStartup<SiteStartup>();
        }
    }

    public class TestSiteConfiguration {
        public string SiteName { get; set; }
    }
}