using Microsoft.AspNet.Builder;
using System.Collections.Generic;
using Orchard.Hosting.Middleware;

namespace Orchard.Test1 {
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