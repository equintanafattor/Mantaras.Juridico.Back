namespace Mantaras.Juridico.Application.Common.Results;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, IReadOnlyCollection<Error> errors)
    {
        IsSuccess = isSuccess;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }

    public IReadOnlyCollection<Error> Errors { get; }

    public static Result<T> Success(T value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return new Result<T>(true, value, Array.Empty<Error>());
    }

    public static Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return new Result<T>(false, default, new[] { error });
    }

    public static Result<T> Failure(IEnumerable<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);

        var errorList = errors.ToArray();

        if (errorList.Length == 0)
        {
            throw new ArgumentException(
                "Un resultado fallido debe contener al menos un error.",
                nameof(errors)
            );
        }

        return new Result<T>(false, default, errorList);
    }
}
