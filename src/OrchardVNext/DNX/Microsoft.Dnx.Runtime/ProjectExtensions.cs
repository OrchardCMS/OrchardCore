// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.Dnx.Compilation;

namespace Microsoft.Dnx.Runtime {
    public static class ProjectExtensions {
        internal static CompilationProjectContext ToCompilationContext(this Project project, CompilationTarget target) {
            Debug.Assert(string.Equals(target.Name, project.Name, StringComparison.Ordinal), "The provided target should be for the current project!");
            return new CompilationProjectContext(
                target,
                project.ProjectDirectory,
                project.ProjectFilePath,
                project.Version.GetNormalizedVersionString(),
                project.AssemblyFileVersion,
                project.EmbedInteropTypes,
                project.Files.GetCompilationFiles(),
                project.GetCompilerOptions(target.TargetFramework, target.Configuration));
        }
    }
}
