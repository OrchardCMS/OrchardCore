using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Orchard.Scripting
{
    public class CommonGeneratorMethods : IGlobalMethodProvider
    {
        private static GlobalMethod Base64 = new GlobalMethod
        {
            Name = "base64",
            Method = serviceProvider => (Func<string, object>)(encoded =>
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
            })
        };

        private static GlobalMethod Html = new GlobalMethod
        {
            Name = "html",
            Method = serviceProvider => (Func<string, object>)(encoded =>
            {
                return WebUtility.HtmlDecode(encoded);
            })
        };

        public IEnumerable<GlobalMethod> GetMethods()
        {
            return new [] { Base64, Html };
        }
    }
}
