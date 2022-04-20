using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.Deployment.Remote.Models;

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
