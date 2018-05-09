using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Mvc
{
    public class TagHelperApplicationPart : ApplicationPart, IApplicationPartTypeProvider
    {
        private Assembly _assembly;

        public TagHelperApplicationPart(Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
        }

        public override string Name => _assembly.GetName().Name + ".TagHelpers";

        public IEnumerable<TypeInfo> Types => _assembly.ExportedTypes
            .Select(t => t.GetTypeInfo()).Where(t => t.IsSubclassOf(typeof(TagHelper)));
    }
}
