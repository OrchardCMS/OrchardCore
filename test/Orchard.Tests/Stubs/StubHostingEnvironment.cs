using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Orchard.Tests.Stubs
{
    public class StubHostingEnvironment : IHostingEnvironment
    {
        public string EnvironmentName { get; set; } = "Stub";

        public string ApplicationName { get; set; }

        public string WebRootPath { get; set; }
           = Directory.GetCurrentDirectory();

        public IFileProvider WebRootFileProvider { get; set; }
            = new PhysicalFileProvider(Directory.GetCurrentDirectory());

        public string ContentRootPath { get; set; }
           = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; }
            = new PhysicalFileProvider(Directory.GetCurrentDirectory());
    }
}