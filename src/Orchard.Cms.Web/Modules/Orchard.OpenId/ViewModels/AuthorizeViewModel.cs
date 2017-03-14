using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Orchard.OpenId.ViewModels
{
    public class AuthorizeViewModel
    {
        public string ApplicationName { get; set; }

        public string RequestId { get; set; }

        public string Scope { get; set; }
    }
}
