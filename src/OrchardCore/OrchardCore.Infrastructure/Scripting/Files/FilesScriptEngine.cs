using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace OrchardCore.Scripting.Files
{
    public class FilesScriptEngine : IScriptingEngine
    {
        public string Prefix => "file";

        public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
        {
            return new FilesScriptScope(fileProvider, basePath);
        }

        public async Task<object> EvaluateAsync(IScriptingScope scope, string script)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            if (!(scope is FilesScriptScope fileScope))
            {
                throw new ArgumentException($"Expected a scope of type {nameof(FilesScriptScope)}", nameof(scope));
            }

            if (script.StartsWith("text('") && script.EndsWith("')"))
            {
                var filePath = script.Substring(6, script.Length - 8);
                var fileInfo = fileScope.FileProvider.GetRelativeFileInfo(fileScope.BasePath, filePath);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException(filePath);
                }

                using (var fileStream = fileInfo.CreateReadStream())
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        return await streamReader.ReadToEndAsync();
                    }
                }
            }
            else if (script.StartsWith("base64('") && script.EndsWith("')"))
            {
                var filePath = script.Substring(8, script.Length - 10);
                var fileInfo = fileScope.FileProvider.GetRelativeFileInfo(fileScope.BasePath, filePath);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException(filePath);
                }

                using (var fileStream = fileInfo.CreateReadStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(ms);
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            else
            {
                throw new ArgumentException($"Unknown command '{script}'");
            }
        }
    }
}
