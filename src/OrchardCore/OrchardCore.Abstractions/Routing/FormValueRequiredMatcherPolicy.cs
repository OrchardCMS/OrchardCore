using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace OrchardCore.Routing
{
    public class FormValueRequiredMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy, IEndpointComparerPolicy
    {
        public FormValueRequiredMatcherPolicy()
        {
        }

        public override int Order => Int32.MinValue + 100;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            for (var i = 0; i < endpoints.Count; i++)
            {
                var action = endpoints[i].Metadata.GetMetadata<ActionDescriptor>();

                if (action != null)
                {
                    for (var n = 0; n < action.EndpointMetadata.Count; n++)
                    {
                        if (action.EndpointMetadata[n] is FormValueRequiredAttribute)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public Task ApplyAsync(HttpContext httpContext, CandidateSet candidates)
        {
            if (!HttpMethods.IsPost(httpContext.Request.Method))
            {
                return Task.CompletedTask;
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                if (!candidates.IsValidCandidate(i))
                {
                    continue;
                }

                var required = candidates[i].Endpoint.Metadata.GetMetadata<FormValueRequiredAttribute>();

                if (required == null)
                {
                    continue;
                }

                var value = httpContext.Request.Form[required.FormKey];

                candidates.SetValidity(i, !String.IsNullOrEmpty(value));
            }

            return Task.CompletedTask;
        }

        public IComparer<Endpoint> Comparer => new FormValueRequiredEndpointComparer();

        private class FormValueRequiredEndpointComparer : EndpointMetadataComparer<FormValueRequiredAttribute>
        {
            protected override int CompareMetadata(FormValueRequiredAttribute x, FormValueRequiredAttribute y)
            {
                return base.CompareMetadata(x, y);
            }
        }
    }
}
