using System.Net;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Diagnostics.ViewModels;

public class ErrorShapeViewModel : ShapeViewModel
{
    public int? Status { get; set; }

    public ErrorShapeViewModel(HttpStatusCode statusCode)
    {
        Status = (int)statusCode;
        Metadata.Type = "Error";

        // Assign an alternative for every 'StatusCode' to enable the customization of the display for each Status.
        // In case of a fallback, we'll utilize the 'Error' shape.
        Metadata.Alternates.Add(statusCode.ToString());
    }
}
