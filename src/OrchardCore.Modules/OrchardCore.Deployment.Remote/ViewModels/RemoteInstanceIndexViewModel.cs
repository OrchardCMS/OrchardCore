using OrchardCore.Deployment.Remote.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Deployment.Remote.ViewModels
{
    public class RemoteInstanceIndexViewModel
    {
        public List<RemoteInstance> RemoteInstances { get; set; }

        public ContentOptions Options { get; set; } = new ContentOptions();

        [BindNever]
        public dynamic Pager { get; set; }
    }
}
