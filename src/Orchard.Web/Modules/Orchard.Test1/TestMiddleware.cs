using Microsoft.AspNet.Builder;
using System.Collections.Generic;
using Orchard.Hosting.Middleware;

namespace Orchard.Test1 {
    public class TestMiddleware : IMiddlewareProvider {

        public IEnumerable<MiddlewareRegistration> GetMiddlewares() {
            yield return new MiddlewareRegistration {
                Configure = app => app.Use(async (context, next) => {
                    if (!context.Response.Headers.ContainsKey("Middleware")) {
                        context.Response.Headers.Add("Middleware", "Hello Orchard");
                    }

                    await next();
                }),
                Priority = "10"
            };
        }
    }
}