using System;
using System.Reflection;

namespace OrchardVNext.FileSystems.Dependencies {
    /// <summary>
    /// Abstraction over the folder configued in web.config as an additional 
    /// location to load assemblies from. This assumes a local physical file system,
    /// since Orchard will need to store assembly files locally.
    /// </summary>
    public interface IAssemblyProbingFolder : ISingletonDependency {
        /// <summary>
        /// Return "true" if the assembly corresponding to "moduleName" is
        /// present in the folder.
        /// </summary>
        bool AssemblyExists(string moduleName);

        /// <summary>
        /// Return the last modification date of the assembly corresponding
        /// to "moduleName". The assembly must be exist on disk, otherwise an
        /// exception is thrown.
        /// </summary>
        DateTime GetAssemblyDateTimeUtc(string moduleName);

        /// <summary>
        /// Return the virtual path of the assembly (optional)
        /// </summary>
        string GetAssemblyVirtualPath(string moduleName);

        /// <summary>
        /// Load the assembly corresponding to "moduleName" if the assembly file
        /// is present in the folder.
        /// </summary>
        Assembly LoadAssembly(string moduleName);

        /// <summary>
        /// Ensure the assembly corresponding to "moduleName" is removed from the folder
        /// </summary>
        void DeleteAssembly(string moduleName);

        /// <summary>
        /// Store an assembly corresponding to "moduleName" from the given fileName
        /// </summary>
        void StoreAssembly(string moduleName, string fileName);
    }
}