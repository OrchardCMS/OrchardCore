using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.ViewModels
{
    public class LogoutViewModel
    {
        [BindNever]
        public IDictionary<string, string> Parameters { get; set; }
    }
}
