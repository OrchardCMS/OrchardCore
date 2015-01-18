using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using OrchardVNext.Middleware;
using System;
using System.Collections.Generic;

namespace OrchardVNext.Test1 {
    public class TestMiddleware : IMiddlewareProvider {

        public IEnumerable<MiddlewareRegistration> GetMiddlewares() {
            yield return new MiddlewareRegistration {
                Configure = app => app.Use(async (context, next) => {
                    context.Response.Headers.Append("Middleware", "Hello Orchard");

                    await next();
                }),
                Priority = "10"
            };
        }
    }
}