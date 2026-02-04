namespace Tips_Player.Infrastructure;

/// <summary>
/// Represents the result of an operation that can either succeed or fail.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
        {
            throw new InvalidOperationException("A successful result cannot have an error.");
        }

        if (!isSuccess && error == Error.None)
        {
            throw new InvalidOperationException("A failed result must have an error.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Gets whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets whether the operation failed.
    /// </summary>
    public bool IsFailure => !IsSuccess;

    /// <summary>
    /// Gets the error if the operation failed.
    /// </summary>
    public Error Error { get; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new(true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Creates a successful result with a value.
    /// </summary>
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);

    /// <summary>
    /// Creates a result from a nullable value.
    /// </summary>
    public static Result<TValue> Create<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Result<TValue> : Result
{
    private readonly TValue? _value;

    protected internal Result(TValue? value, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the value if the operation was successful.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when accessing value on a failed result.</exception>
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access value of a failed result.");

    /// <summary>
    /// Gets the value or the default value if the operation failed.
    /// </summary>
    public TValue? ValueOrDefault => _value;

    /// <summary>
    /// Implicit conversion from value to successful result.
    /// </summary>
    public static implicit operator Result<TValue>(TValue? value) => Create(value);

    /// <summary>
    /// Implicit conversion from error to failed result.
    /// </summary>
    public static implicit operator Result<TValue>(Error error) => Failure<TValue>(error);

    /// <summary>
    /// Maps the value to a new type if successful.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<TValue, TNew> mapper)
    {
        return IsSuccess
            ? Result.Success(mapper(_value!))
            : Result.Failure<TNew>(Error);
    }

    /// <summary>
    /// Binds to another result if successful.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<TValue, Result<TNew>> binder)
    {
        return IsSuccess
            ? binder(_value!)
            : Result.Failure<TNew>(Error);
    }

    /// <summary>
    /// Executes an action on success or failure.
    /// </summary>
    public Result<TValue> Match(Action<TValue> onSuccess, Action<Error> onFailure)
    {
        if (IsSuccess)
        {
            onSuccess(_value!);
        }
        else
        {
            onFailure(Error);
        }

        return this;
    }
}

/// <summary>
/// Extension methods for Result types.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts a task returning a value to a task returning a result.
    /// </summary>
    public static async Task<Result<T>> ToResultAsync<T>(this Task<T?> task)
    {
        try
        {
            var value = await task;
            return Result.Create(value);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Executes a task and wraps the result.
    /// </summary>
    public static async Task<Result> TryAsync(Func<Task> action)
    {
        try
        {
            await action();
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(Error.FromException(ex));
        }
    }

    /// <summary>
    /// Executes a task and wraps the result with a value.
    /// </summary>
    public static async Task<Result<T>> TryAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var result = await action();
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            return Result.Failure<T>(Error.FromException(ex));
        }
    }
}
