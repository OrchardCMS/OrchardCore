using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microsoft.AspNetCore.Mvc.Filters;

public static class ResultExecutingContextExtensions
{
    public static bool IsViewOrPageResult(this ResultExecutingContext context)
        => context.IsViewResult() || context.IsPageResult();

    public static bool IsViewResult(this ResultExecutingContext context)
        => context.Result is ViewResult;

    public static bool IsPageResult(this ResultExecutingContext context)
        => context.Result is PageResult;
}
