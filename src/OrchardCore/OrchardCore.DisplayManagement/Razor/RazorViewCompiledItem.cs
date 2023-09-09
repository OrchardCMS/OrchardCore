using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Hosting;

namespace OrchardCore.DisplayManagement.Razor
{
    public sealed class RazorViewCompiledItem : RazorCompiledItem
    {
        private object[] _metadata;

        public RazorViewCompiledItem(Type type, string identifier, object[] metadata = null)
        {
            Type = type;
            Identifier = identifier;
        }

        public override string Identifier { get; }

        public override string Kind => MvcViewDocumentClassifierPass.MvcViewDocumentKind;

        public override IReadOnlyList<object> Metadata => _metadata ??= Type.GetCustomAttributes(inherit: true);

        public override Type Type { get; }
    }
}
