using System;
using System.Collections.Generic;
using System.Linq;

namespace OrchardCore.Security
{
    public class PermissionsPolicyOptions
    {
        public PermissionsPolicyOptions() => Values = new List<PermissionsPolicyValue>();

        public ICollection<PermissionsPolicyValue> Values { get; set; }

        public PermissionsPolicyOriginValue Origin { get; set; } = PermissionsPolicyOriginValue.Self;

        public override string ToString() => Values.Count == 0
            ? String.Empty
            : String.Join(',', Values.Select(v => $"{v}={Origin}"));
    }
}
