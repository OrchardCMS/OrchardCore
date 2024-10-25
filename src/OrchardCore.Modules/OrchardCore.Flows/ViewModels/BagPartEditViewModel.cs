using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Flows.Models;

namespace OrchardCore.Flows.ViewModels;

public class BagPartEditViewModel
{
    public string[] Prefixes { get; set; } = [];
    public string[] ContentTypes { get; set; } = [];
    public string[] ContentItems { get; set; } = [];

    [BindNever]
    public BagPart BagPart { get; set; }

    [IgnoreDataMember]
    [BindNever]
    public IUpdateModel Updater { get; set; }

    [BindNever]
    public IEnumerable<ContentTypeDefinition> ContainedContentTypeDefinitions { get; set; }

    [BindNever]
    public IEnumerable<BagPartWidgetViewModel> AccessibleWidgets { get; set; }

    [BindNever]
    public ContentTypePartDefinition TypePartDefinition { get; set; }
}
