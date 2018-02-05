using System;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Workflows.Services
{
    public class Result
    {
        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(value, true, null);
        }

        public static Result<T> Fail<T>(LocalizedString errorMessage)
        {
            return new Result<T>(default(T), false, errorMessage);
        }

        internal Result(bool isSuccess, LocalizedString errorMessage)
        {
            IsSuccess = isSuccess;
            ErrorMessage = errorMessage;
        }

        public bool IsSuccess { get; set; }
        public LocalizedString ErrorMessage { get; set; }
    }

    public class Result<T> : Result
    {
        internal Result(T value, bool isSuccess, LocalizedString errorMessage) : base(isSuccess, errorMessage)
        {
            Value = value;
        }

        public T Value { get; }
    }

    public static class ResultExtensions
    {
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> handler)
        {
            if (result.IsSuccess)
            {
                handler(result.Value);
            }

            return result;
        }

        public static Result<T> OnSuccess<T>(this Result<T> result, Action handler)
        {
            if (result.IsSuccess)
            {
                handler();
            }

            return result;
        }

        public static Result<T> OnFailure<T>(this Result<T> result, Action<T, LocalizedString> handler)
        {
            if (!result.IsSuccess)
            {
                handler(result.Value, result.ErrorMessage);
            }

            return result;
        }

        public static Result<T> OnFailure<T>(this Result<T> result, Action<LocalizedString> handler)
        {
            if (!result.IsSuccess)
            {
                handler(result.ErrorMessage);
            }

            return result;
        }

        public static Result<T> OnFailure<T>(this Result<T> result, Action handler)
        {
            if (!result.IsSuccess)
            {
                handler();
            }

            return result;
        }

        public static Result<T> OnEither<T>(this Result<T> result, Action<T, LocalizedString> handler)
        {
            handler(result.Value, result.ErrorMessage);

            return result;
        }

        public static Result<T> OnEither<T>(this Result<T> result, Action handler)
        {
            handler();

            return result;
        }
    }
}
