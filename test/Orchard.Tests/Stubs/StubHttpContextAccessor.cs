using System;
using Microsoft.AspNetCore.Http;

namespace Orchard.Tests.Stubs
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