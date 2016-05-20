// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.DotNet.ProjectModel;
using Microsoft.DotNet.ProjectModel.Files;

namespace Microsoft.DotNet.Tools.Compiler
{
    public static class CompilerUtil
    {
        public static string ResolveLanguageId(ProjectContext context)
        {
            var languageId = context.ProjectFile.AnalyzerOptions?.LanguageId;
            if (languageId == null)
            {
                languageId = context.ProjectFile.GetSourceCodeLanguage();
            }

            return languageId;
        }

        public static IEnumerable<string> GetCompilationSources(ProjectContext project, CommonCompilerOptions compilerOptions)
        {
            if (compilerOptions.CompileInclude == null)
            {
                return project.ProjectFile.Files.SourceFiles;
            }

            var includeFiles = IncludeFilesResolver.GetIncludeFiles(compilerOptions.CompileInclude, "/", diagnostics: null);

            return includeFiles.Select(f => f.SourcePath);
        }
    }
}