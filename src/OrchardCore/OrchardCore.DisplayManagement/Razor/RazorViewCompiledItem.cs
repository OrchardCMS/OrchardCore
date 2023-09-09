using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Hosting;

namespace OrchardCore.DisplayManagement.Razor
{
    public class RazorViewCompiledItem : RazorCompiledItem
    {
        public RazorViewCompiledItem(Type type, string identifier, object[] metadata = null)
        {
            Type = type;
            Identifier = identifier;
            Metadata = metadata ?? Array.Empty<object>();
        }

        public override string Identifier { get; }

        public override string Kind => MvcViewDocumentClassifierPass.MvcViewDocumentKind;

        public override IReadOnlyList<object> Metadata { get; }

        public override Type Type { get; }
    }
}
