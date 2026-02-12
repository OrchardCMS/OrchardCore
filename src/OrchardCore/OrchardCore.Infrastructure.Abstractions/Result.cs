using Microsoft.Extensions.Localization;

namespace OrchardCore.Infrastructure;

public class Result
{
    private static readonly Result _success = new Result { Succeeded = true };
    private readonly List<ResultError> _errors = [];

    private Result()
    {
    }

    protected Result(bool succeeded, List<ResultError> errors)
    {
        Succeeded = succeeded;
        _errors = errors;
    }

    public IEnumerable<ResultError> Errors => _errors;

    public bool Succeeded { get; protected set; }

    public static Result Success() => _success;

    public static Result<TValue> Success<TValue>(TValue value)
        => new Result<TValue>(value, true, null);

    public static Result Failed(params ResultError[] errors)
    {
        var result = new Result { Succeeded = false };

        if (errors is not null)
        {
            result._errors.AddRange(errors);
        }

        return result;
    }

    public static Result Failed(LocalizedString error) => Failed(new ResultError
    {
        Message = error,
    });

    public static Result<TValue> Failed<TValue>(params ResultError[] errors)
        => new Result<TValue>(default, false, errors.ToList());
}
