namespace Tips_Player.Infrastructure;

/// <summary>
/// Represents an error that occurred during an operation.
/// </summary>
/// <param name="Code">A unique error code for categorization.</param>
/// <param name="Message">A human-readable error message.</param>
public sealed record Error(string Code, string Message)
{
    /// <summary>
    /// Represents no error (success state).
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);

    /// <summary>
    /// Creates an error with a null value.
    /// </summary>
    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided.");

    /// <summary>
    /// Creates an error for validation failures.
    /// </summary>
    public static Error Validation(string message) => new("Error.Validation", message);

    /// <summary>
    /// Creates an error for not found resources.
    /// </summary>
    public static Error NotFound(string message) => new("Error.NotFound", message);

    /// <summary>
    /// Creates an error for file operations.
    /// </summary>
    public static Error FileOperation(string message) => new("Error.FileOperation", message);

    /// <summary>
    /// Creates an error for media operations.
    /// </summary>
    public static Error MediaOperation(string message) => new("Error.MediaOperation", message);

    /// <summary>
    /// Creates an error from an exception.
    /// </summary>
    public static Error FromException(Exception ex) => new("Error.Exception", ex.Message);

    /// <summary>
    /// Implicit conversion to string returns the message.
    /// </summary>
    public static implicit operator string(Error error) => error.Message;
}
