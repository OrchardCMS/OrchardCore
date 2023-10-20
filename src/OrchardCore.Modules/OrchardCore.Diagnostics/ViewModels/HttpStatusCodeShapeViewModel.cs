using System.Net;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Diagnostics.ViewModels;

public class HttpStatusCodeShapeViewModel : ShapeViewModel
{
    public int? Status { get; }

    public HttpStatusCodeShapeViewModel(HttpStatusCode statusCode)
    {
        Status = (int)statusCode;
        Metadata.Type = "HttpStatusCode";

        // The type name is 'HttpStatusCode', so any Http status code will be handled by the 'HttpStatusCode.cshtml' or 'HttpStatusCode.liquid'.
        // However, assign an alternative for every 'StatusCode' to enable the customization for each Status (ex. 'NotFound.cshtml' or 'NotFound.liquid').
        Metadata.Alternates.Add(statusCode.ToString());
    }
}
