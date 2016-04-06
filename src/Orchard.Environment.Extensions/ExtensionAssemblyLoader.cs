using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Orchard.DependencyInjection;
using Orchard.Environment.Extensions.Loaders;

namespace Orchard.Environment.Extensions
{
    //public class ExtensionAssemblyLoader : IExtensionAssemblyLoader
    //{
    //    private readonly IApplicationEnvironment _applicationEnvironment;
    //    private readonly IAssemblyLoadContextAccessor _assemblyLoadContextAccessor;
    //    private readonly IRuntimeEnvironment _runtimeEnvironment;
    //    private readonly IOrchardLibraryManager _libraryManager;
    //    private string _path;

    //    public ExtensionAssemblyLoader(
    //        IApplicationEnvironment applicationEnvironment,
    //        //ICache cache,
    //        IAssemblyLoadContextAccessor assemblyLoadContextAccessor,
    //        IRuntimeEnvironment runtimeEnvironment,
    //        IOrchardLibraryManager libraryManager)
    //    {
    //        _applicationEnvironment = applicationEnvironment;
    //        //_cache = cache;
    //        _assemblyLoadContextAccessor = assemblyLoadContextAccessor;
    //        _runtimeEnvironment = runtimeEnvironment;
    //        _libraryManager = libraryManager;
    //    }

    //    public IExtensionAssemblyLoader WithPath(string path)
    //    {
    //        _path = path;
    //        return this;
    //    }

    //    public Assembly Load(AssemblyName assemblyName)
    //    {
    //        var reference = _libraryManager.GetMetadataReference(assemblyName.Name);

    //        if (reference != null && reference is MetadataFileReference)
    //        {
    //            var fileReference = (MetadataFileReference)reference;

    //            var assembly = _assemblyLoadContextAccessor
    //                .Default
    //                .LoadFile(fileReference.Path);

    //            return assembly;
    //        }

    //        return null;
    //    }

    //    public IntPtr LoadUnmanagedLibrary(string name)
    //    {
    //        return IntPtr.Zero;
    //    }
    //}
}
