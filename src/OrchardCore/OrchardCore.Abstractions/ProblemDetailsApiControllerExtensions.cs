using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace Microsoft.AspNetCore.Mvc;

public class ProblemDetailsApiLocalization();

public static class ProblemDetailsApiControllerExtensions
{
    /// <summary>
    /// Returns a Problem response for unauthenticated or forbidden requests
    /// when using cookie authentication on an API controller.
    /// </summary>
    public static ActionResult ApiChallengeOrForbidForCookieAuth(this ControllerBase controllerBase)
    {
        var S = controllerBase.HttpContext.RequestServices.GetRequiredService<
            IStringLocalizer<ProblemDetailsApiLocalization>
        >();

        if (controllerBase.User?.Identity?.IsAuthenticated is false)
        {
            return controllerBase.Problem(
                title: S["Unauthorized"],
                detail: S["You must be authenticated to complete this request"],
                statusCode: StatusCodes.Status401Unauthorized
            );
        }

        return controllerBase.Problem(
            title: S["Forbidden"],
            detail: S["You do not have sufficient permissions to complete this request"],
            statusCode: StatusCodes.Status403Forbidden
        );
    }

    /// <summary>
    /// Returns a Problem with a 400 Bad Request status code.
    /// </summary>
    public static ObjectResult ApiBadRequestProblem(
        this ControllerBase controllerBase,
        LocalizedString detail = null,
        LocalizedString title = null
    )
    {
        if (string.IsNullOrEmpty(title))
        {
            var S = controllerBase.HttpContext.RequestServices.GetRequiredService<
                IStringLocalizer<ProblemDetailsApiLocalization>
            >();

            return controllerBase.Problem(
                title: S["Bad request"],
                detail: detail,
                statusCode: StatusCodes.Status400BadRequest
            );
        }

        return controllerBase.Problem(title: title, detail: detail, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Returns a Problem with a 404 Not Found status code.
    /// </summary>
    public static ObjectResult ApiNotFoundProblem(
        this ControllerBase controllerBase,
        LocalizedString detail = null,
        LocalizedString title = null
    )
    {
        if (string.IsNullOrEmpty(title))
        {
            var S = controllerBase.HttpContext.RequestServices.GetRequiredService<
                IStringLocalizer<ProblemDetailsApiLocalization>
            >();

            return controllerBase.Problem(
                title: S["Not Found"],
                detail: detail,
                statusCode: StatusCodes.Status404NotFound
            );
        }

        return controllerBase.Problem(title: title, detail: detail, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Returns a ValidationProblem with a 400 Bad Request status code.
    /// </summary>
    public static ActionResult ApiValidationProblem(
        this ControllerBase controllerBase,
        LocalizedString detail = null,
        LocalizedString title = null,
        ModelStateDictionary modelState = null
    )
    {
        if (string.IsNullOrEmpty(title))
        {
            var S = controllerBase.HttpContext.RequestServices.GetRequiredService<
                IStringLocalizer<ProblemDetailsApiLocalization>
            >();

            return controllerBase.ValidationProblem(
                detail: detail,
                title: S["A validation error occurred."],
                statusCode: StatusCodes.Status400BadRequest,
                modelStateDictionary: modelState
            );
        }

        return controllerBase.ValidationProblem(
            detail: detail,
            title: title,
            statusCode: StatusCodes.Status400BadRequest,
            modelStateDictionary: modelState
        );
    }
}
