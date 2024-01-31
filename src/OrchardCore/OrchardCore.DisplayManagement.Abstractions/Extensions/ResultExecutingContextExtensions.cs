using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microsoft.AspNetCore.Mvc.Filters;

public static class ResultExecutingContextExtensions
{
    public static bool IsViewOrPageResult(this ResultExecutingContext context)
        => context.Result is ViewResult or PageResult;
}
