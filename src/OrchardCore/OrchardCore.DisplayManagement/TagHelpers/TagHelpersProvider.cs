using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.DisplayManagement.TagHelpers
{
    public class TagHelpersProvider : ITagHelpersProvider
    {
        public TagHelpersProvider(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            Types = assembly.ExportedTypes.Select(t => t.GetTypeInfo()).Where(t => t.IsSubclassOf(typeof(TagHelper)));
        }

        public IEnumerable<TypeInfo> Types { get; }
    }
}