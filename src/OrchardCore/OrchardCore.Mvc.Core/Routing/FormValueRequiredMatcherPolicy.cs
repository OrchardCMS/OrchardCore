using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Matching;

namespace OrchardCore.Mvc.Routing
{
    internal class FormValueRequiredMatcherPolicy : MatcherPolicy, IEndpointSelectorPolicy
    {
        public FormValueRequiredMatcherPolicy()
        {
        }

        public override int Order => int.MinValue + 100;

        public bool AppliesToEndpoints(IReadOnlyList<Endpoint> endpoints)
        {
            if (endpoints == null)
            {
                throw new ArgumentNullException(nameof(endpoints));
            }

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

        public Task ApplyAsync(HttpContext httpContext, EndpointSelectorContext context, CandidateSet candidates)
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

                var endpoint = candidates[i].Endpoint;

                var required = endpoint.Metadata.GetMetadata<FormValueRequiredAttribute>();

                if (required == null)
                {
                    continue;
                }

                var value = httpContext.Request.Form[required.FormKey];

                candidates.SetValidity(i, !string.IsNullOrEmpty(value));
            }

            return Task.CompletedTask;
        }
    }
}