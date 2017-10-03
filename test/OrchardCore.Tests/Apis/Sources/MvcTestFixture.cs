// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Tests.Apis.Sources;

namespace Microsoft.AspNetCore.Mvc.FunctionalTests
{
    public class OrchardTestFixture<TStartup> : WebApplicationTestFixture<TStartup>
        where TStartup : class
    {
        public string SiteName => "Sites_" + Guid.NewGuid().ToString().Replace("-", "");
        public string AppData => Path.Combine(EnvironmentHelpers.GetApplicationPath(), "App_Data", SiteName);

        public OrchardTestFixture()
            : base(Path.Combine("test", "WebSites", typeof(TStartup).Assembly.GetName().Name))
        {
        }

        public OrchardTestFixture(string solutionRelativePath)
            : base(solutionRelativePath)
        {
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            if (Directory.Exists(AppData))
            {
                Directory.Delete(AppData, true);
            }

            builder.ConfigureServices(x => x.AddSingleton(new TestSiteConfiguration { SiteName = SiteName }));
        }

        protected override TestServer CreateServer(IWebHostBuilder builder)
        {
            var originalCulture = CultureInfo.CurrentCulture;
            var originalUICulture = CultureInfo.CurrentUICulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-GB");
                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
                return base.CreateServer(builder);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
                CultureInfo.CurrentUICulture = originalUICulture;
            }
        }
    }

    public class TestSiteConfiguration {
        public string SiteName { get; set; }
    }
}