using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

public class Result
{
    public Result()
    {
    }

    protected Result(bool succeeded, IEnumerable<LocalizedString> errors)
    {
        Succeeded = succeeded;
        Errors = errors;
    }

    public IEnumerable<LocalizedString> Errors { get; protected set; }

    public bool Succeeded { get; protected set; }

    public static Result Success() =>  new()
    {
        Succeeded = true,
    };

    public static Result Failed(params LocalizedString[] errors) => new()
    {
        Succeeded = false,
        Errors = errors ?? [],
    };

    public static Result<TValue> Success<TValue>(TValue value)
        => new Result<TValue>(value, true, null);

    public static Result<TValue> Failed<TValue>(params LocalizedString[] errors)
        => new Result<TValue>(default, false, errors);
}
