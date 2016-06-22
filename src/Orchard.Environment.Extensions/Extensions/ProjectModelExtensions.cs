using System.Collections.Generic;
using System.Linq;
using Microsoft.DotNet.ProjectModel.Compilation;

namespace Orchard.Environment.Extensions.ProjectModel
{
    public static class LibraryExporterExtensions {
        public static IEnumerable<LibraryExport> GetAllCompatibleExports(this LibraryExporter exporter) {
            return exporter.GetAllExports().Where(export => export.Library.Compatible);
        }
    }
}
