using Tips_Player.Constants;
using Tips_Player.Models;

namespace Tips_Player.Infrastructure.Validation;

/// <summary>
/// Validates MediaItem instances before processing.
/// </summary>
public static class MediaItemValidator
{
    /// <summary>
    /// Validates a media item for adding to the library.
    /// </summary>
    /// <param name="item">The media item to validate.</param>
    /// <returns>A Result indicating success or failure with error details.</returns>
    public static Result Validate(MediaItem item)
    {
        if (item == null)
        {
            return Result.Failure(Error.NullValue);
        }

        // Validate file path
        var filePathResult = ValidateFilePath(item.FilePath);
        if (filePathResult.IsFailure)
        {
            return filePathResult;
        }

        // Validate title
        var titleResult = ValidateTitle(item.Title);
        if (titleResult.IsFailure)
        {
            return titleResult;
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates a file path.
    /// </summary>
    public static Result ValidateFilePath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Result.Failure(Error.Validation("File path is required."));
        }

        if (filePath.Length > AppConstants.Validation.MaxFilePathLength)
        {
            return Result.Failure(Error.Validation($"File path exceeds maximum length of {AppConstants.Validation.MaxFilePathLength} characters."));
        }

        // Check for invalid path characters
        var invalidChars = Path.GetInvalidPathChars();
        if (filePath.Any(c => invalidChars.Contains(c)))
        {
            return Result.Failure(Error.Validation("File path contains invalid characters."));
        }

        // Check if file exists
        if (!File.Exists(filePath))
        {
            return Result.Failure(Error.NotFound($"File not found: {filePath}"));
        }

        // Check file extension
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (!FileConstants.AudioExtensions.Contains(extension) &&
            !FileConstants.VideoExtensions.Contains(extension))
        {
            return Result.Failure(Error.Validation($"Unsupported file type: {extension}"));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates a media title.
    /// </summary>
    public static Result ValidateTitle(string? title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Result.Failure(Error.Validation("Title is required."));
        }

        if (title.Length > AppConstants.Validation.MaxTitleLength)
        {
            return Result.Failure(Error.Validation($"Title exceeds maximum length of {AppConstants.Validation.MaxTitleLength} characters."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates an artist name.
    /// </summary>
    public static Result ValidateArtist(string? artist)
    {
        // Artist is optional but has length limits
        if (!string.IsNullOrEmpty(artist) && artist.Length > AppConstants.Validation.MaxNameLength)
        {
            return Result.Failure(Error.Validation($"Artist name exceeds maximum length of {AppConstants.Validation.MaxNameLength} characters."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates an album name.
    /// </summary>
    public static Result ValidateAlbum(string? album)
    {
        // Album is optional but has length limits
        if (!string.IsNullOrEmpty(album) && album.Length > AppConstants.Validation.MaxNameLength)
        {
            return Result.Failure(Error.Validation($"Album name exceeds maximum length of {AppConstants.Validation.MaxNameLength} characters."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Validates a collection of media items.
    /// </summary>
    /// <param name="items">The items to validate.</param>
    /// <returns>A list of validation results, one per item.</returns>
    public static IEnumerable<(MediaItem Item, Result Result)> ValidateAll(IEnumerable<MediaItem> items)
    {
        return items.Select(item => (item, Validate(item)));
    }

    /// <summary>
    /// Filters a collection to only include valid items.
    /// </summary>
    public static IEnumerable<MediaItem> FilterValid(IEnumerable<MediaItem> items)
    {
        return items.Where(item => Validate(item).IsSuccess);
    }
}
