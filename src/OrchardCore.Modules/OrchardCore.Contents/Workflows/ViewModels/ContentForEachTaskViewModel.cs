using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Workflows.Abstractions.Models;
using OrchardCore.Workflows.Activities;
using OrchardCore.Workflows.Services;
using OrchardCore.Workflows.Display;
using OrchardCore.Workflows.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Workflows;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OrchardCore.Contents.Workflows.Activities;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Contents.Workflows.ViewModels
{
    public class ContentForEachTaskViewModel
    {
        public bool QueriesEnabled { get; set; }
        public bool UseQuery { get; set; }
        public string ContentType { get; set; }
        public string Query { get; set; }
        public string Parameters { get; set; }
        public bool PublishedOnly { get; set; }
        public int Take { get; set; }
        
        [BindNever]
        public IList<SelectListItem> AvailableContentTypes { get; set; }

        [BindNever]
        public IList<SelectListItem> Queries { get; set; }
    }

}
