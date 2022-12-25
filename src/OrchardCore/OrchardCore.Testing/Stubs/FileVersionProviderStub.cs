using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OrchardCore.Testing.Stubs;

public class FileVersionProviderStub : IFileVersionProvider
{
    public string AddFileVersionToPath(PathString requestPathBase, string path) => path;
}
