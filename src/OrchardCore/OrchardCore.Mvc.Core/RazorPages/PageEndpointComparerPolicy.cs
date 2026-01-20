using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace OrchardCore.Mvc.RazorPages
{
    public sealed class PageEndpointComparerPolicy : MatcherPolicy, IEndpointComparerPolicy
    {
        public override int Order => -1000;

        public IComparer<Endpoint> Comparer => new PageEndpointComparer();

        private class PageEndpointComparer : EndpointMetadataComparer<PageActionDescriptor>
        {
            protected override int CompareMetadata(PageActionDescriptor x, PageActionDescriptor y)
            {
                return base.CompareMetadata(x, y);
            }
        }
    }
}
