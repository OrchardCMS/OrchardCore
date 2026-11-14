using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Deployment.Remote.ViewModels;

public class EditRemoteClientViewModel
{
    public string Id { get; set; }
    public string ClientName { get; set; }
    public string ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the editor of the remote client, built by the display drivers of <see cref="EditRemoteClientViewModel"/>.
    /// </summary>
    [BindNever]
    public IShape Editor { get; set; }
}
