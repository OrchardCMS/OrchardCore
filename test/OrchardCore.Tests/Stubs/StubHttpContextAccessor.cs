using System;
using Microsoft.AspNetCore.Http;

namespace OrchardCore.Stubs.Tests
{
    public class StubHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
