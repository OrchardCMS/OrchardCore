using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.ProjectModel.Graph;
using Microsoft.Extensions.DependencyModel;

namespace Orchard.Environment.Extensions.DependencyModel
{
    public static class DependencyContextExtensions
    {
        public static IEnumerable<CompilationLibrary> GetProjectTypeCompileLibraries(this DependencyContext context)
        {
            return context.CompileLibraries.Where(x => x.Type.Equals(LibraryType.Project.ToString(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
