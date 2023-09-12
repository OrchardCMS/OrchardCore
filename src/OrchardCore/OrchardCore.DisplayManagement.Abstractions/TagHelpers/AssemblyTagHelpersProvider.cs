using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    public class AssemblyTagHelpersProvider : ITagHelpersProvider
    {
        private readonly Assembly _assembly;

        public AssemblyTagHelpersProvider(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            _assembly = assembly;
        }

        public IEnumerable<Type> GetTypes() => _assembly.ExportedTypes.Where(t => t.IsSubclassOf(typeof(TagHelper)));
    }

    public class TagHelpersProvider<T> : ITagHelpersProvider
    {
        public IEnumerable<Type> GetTypes() => new Type[] { typeof(T) };
    }
}
