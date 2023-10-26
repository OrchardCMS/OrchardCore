using System.Net;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Diagnostics.ViewModels;

public class HttpStatusCodeShapeViewModel : ShapeViewModel
{
    private const string ShapeType = "HttpError";

    public int Code { get; set; }

    public HttpStatusCode HttpStatusCode { get; }

    public HttpStatusCodeShapeViewModel(HttpStatusCode statusCode)
    {
        HttpStatusCode = statusCode;
        Code = (int)statusCode;

        Metadata.Type = ShapeType;
        // The type name is 'HttpError', which means any Http status code will be handled by the 'HttpError.cshtml' or 'HttpError.liquid'.
        // However, assign an alternative for every status-code to enable the customization for each status.

        // (ex. 'HttpError-404.cshtml' or 'HttpError-404.liquid')
        Metadata.Alternates.Add($"{ShapeType}__{Code}");

        // (ex. 'HttpError-NotFound.cshtml' or 'HttpError-NotFound.liquid')
        Metadata.Alternates.Add($"{ShapeType}__{statusCode}");
    }
}
