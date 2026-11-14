using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.Remote.ViewModels;

public class EditRemoteInstanceViewModel
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string ClientName { get; set; }
    public string ApiKey { get; set; }
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the editor of the remote instance, built by the display drivers of <see cref="EditRemoteInstanceViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
