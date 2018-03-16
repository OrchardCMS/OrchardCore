using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Scripting.Files
{
    public class FilesScriptEngine : IScriptingEngine
    {
		public FilesScriptEngine(IStringLocalizer<FilesScriptEngine> localizer)
        {
            S = localizer;
        }

        IStringLocalizer S { get; }

        public LocalizedString Name => S["Files"];

        public string Prefix => "file";

        public IScriptingScope CreateScope(IEnumerable<GlobalMethod> methods, IServiceProvider serviceProvider, IFileProvider fileProvider, string basePath)
        {
            return new FilesScriptScope(fileProvider, basePath);
        }

        public object Evaluate(IScriptingScope scope, string script)
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
                var fileInfo = fileScope.FileProvider.GetFileInfo(fileScope.GetRelativeFile(filePath));
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException(filePath);
                }

                using (var fileStream = fileInfo.CreateReadStream())
                {
                    using (var streamReader = new StreamReader(fileStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
            else if(script.StartsWith("base64('") && script.EndsWith("')"))
            {
                var filePath = script.Substring(8, script.Length - 10);
                var fileInfo = fileScope.FileProvider.GetFileInfo(fileScope.GetRelativeFile(filePath));
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException(filePath);
                }

                using (var fileStream = fileInfo.CreateReadStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
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
