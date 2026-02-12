namespace OrchardCore.Infrastructure;

public class Result<TValue> : Result
{
    public TValue Value { get; set; }

    protected internal Result(TValue value, bool succeeded, List<ResultError> errors)
        : base(succeeded, errors) => Value = value;
}
