using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Benchmark.Support
{
    internal sealed class FakeWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Benchmark";

        public IFileProvider ContentRootFileProvider { get; set; } = new FakeFileProvider();

        public string ContentRootPath { get; set; }

        public string EnvironmentName { get; set; }

        public IFileProvider WebRootFileProvider { get; set; } = new FakeFileProvider();

        public string WebRootPath { get; set; }
    }
}
