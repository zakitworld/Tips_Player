namespace Tips_Player.Tests.Services;

/// <summary>
/// Tests for library service functionality.
/// Note: These tests use local test doubles since the MAUI project cannot be directly referenced.
/// In a production setup, extract shared models to a separate .NET Standard library.
/// </summary>
public class LibraryServiceTests
{
    [Fact]
    public void MediaItems_ShouldBeInitializedAsEmptyCollection()
    {
        // Arrange
        var sut = new TestLibraryService();

        // Assert
        sut.MediaItems.Should().NotBeNull();
        sut.MediaItems.Should().BeEmpty();
    }

    [Fact]
    public async Task AddItemsAsync_ShouldAddSingleItem()
    {
        // Arrange
        var sut = new TestLibraryService();
        var mediaItem = CreateTestMediaItem("Test Song", TestMediaType.Audio);

        // Act
        await sut.AddItemsAsync(new[] { mediaItem });

        // Assert
        sut.MediaItems.Should().HaveCount(1);
        sut.MediaItems.Should().Contain(mediaItem);
    }

    [Fact]
    public async Task AddItemsAsync_ShouldAddMultipleItems()
    {
        // Arrange
        var sut = new TestLibraryService();
        var items = new[]
        {
            CreateTestMediaItem("Song 1", TestMediaType.Audio),
            CreateTestMediaItem("Song 2", TestMediaType.Audio),
            CreateTestMediaItem("Video 1", TestMediaType.Video)
        };

        // Act
        await sut.AddItemsAsync(items);

        // Assert
        sut.MediaItems.Should().HaveCount(3);
    }

    [Fact]
    public async Task AddItemsAsync_ShouldNotAddDuplicatesByFilePath()
    {
        // Arrange
        var sut = new TestLibraryService();
        var item1 = CreateTestMediaItem("Song 1", TestMediaType.Audio, "C:\\test\\song.mp3");
        var item2 = CreateTestMediaItem("Song 1 Duplicate", TestMediaType.Audio, "C:\\test\\song.mp3");

        // Act
        await sut.AddItemsAsync(new[] { item1 });
        await sut.AddItemsAsync(new[] { item2 });

        // Assert
        sut.MediaItems.Should().HaveCount(1);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItem()
    {
        // Arrange
        var sut = new TestLibraryService();
        var mediaItem = CreateTestMediaItem("Test Song", TestMediaType.Audio);
        await sut.AddItemsAsync(new[] { mediaItem });

        // Act
        await sut.RemoveItemAsync(mediaItem);

        // Assert
        sut.MediaItems.Should().BeEmpty();
    }

    [Fact]
    public async Task ClearLibraryAsync_ShouldRemoveAllItems()
    {
        // Arrange
        var sut = new TestLibraryService();
        var items = new[]
        {
            CreateTestMediaItem("Song 1", TestMediaType.Audio),
            CreateTestMediaItem("Song 2", TestMediaType.Audio)
        };
        await sut.AddItemsAsync(items);

        // Act
        await sut.ClearLibraryAsync();

        // Assert
        sut.MediaItems.Should().BeEmpty();
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ShouldToggleFavoriteStatus()
    {
        // Arrange
        var sut = new TestLibraryService();
        var mediaItem = CreateTestMediaItem("Test Song", TestMediaType.Audio);
        mediaItem.IsFavorite = false;
        await sut.AddItemsAsync(new[] { mediaItem });

        // Act
        await sut.ToggleFavoriteAsync(mediaItem);

        // Assert
        mediaItem.IsFavorite.Should().BeTrue();

        // Act again
        await sut.ToggleFavoriteAsync(mediaItem);

        // Assert
        mediaItem.IsFavorite.Should().BeFalse();
    }

    [Fact]
    public async Task RecordPlayAsync_ShouldIncrementPlayCount()
    {
        // Arrange
        var sut = new TestLibraryService();
        var mediaItem = CreateTestMediaItem("Test Song", TestMediaType.Audio);
        mediaItem.PlayCount = 0;
        await sut.AddItemsAsync(new[] { mediaItem });

        // Act
        await sut.RecordPlayAsync(mediaItem);

        // Assert
        mediaItem.PlayCount.Should().Be(1);
    }

    [Fact]
    public async Task RecordPlayAsync_ShouldUpdateLastPlayedDate()
    {
        // Arrange
        var sut = new TestLibraryService();
        var mediaItem = CreateTestMediaItem("Test Song", TestMediaType.Audio);
        var beforePlay = DateTime.Now;
        await sut.AddItemsAsync(new[] { mediaItem });

        // Act
        await sut.RecordPlayAsync(mediaItem);

        // Assert
        mediaItem.LastPlayedDate.Should().NotBeNull();
        mediaItem.LastPlayedDate.Should().BeOnOrAfter(beforePlay);
    }

    [Fact]
    public void RefreshCollections_ShouldSeparateSongsAndVideos()
    {
        // Arrange
        var sut = new TestLibraryService();
        var song = CreateTestMediaItem("Song", TestMediaType.Audio);
        var video = CreateTestMediaItem("Video", TestMediaType.Video);
        sut.MediaItems.Add(song);
        sut.MediaItems.Add(video);

        // Act
        sut.RefreshCollections();

        // Assert
        sut.Songs.Should().Contain(song);
        sut.Songs.Should().NotContain(video);
        sut.Videos.Should().Contain(video);
        sut.Videos.Should().NotContain(song);
    }

    private static TestMediaItem CreateTestMediaItem(string title, TestMediaType mediaType, string? filePath = null)
    {
        return new TestMediaItem
        {
            Title = title,
            MediaType = mediaType,
            FilePath = filePath ?? $"C:\\test\\{title.Replace(" ", "_")}.{(mediaType == TestMediaType.Audio ? "mp3" : "mp4")}",
            Artist = "Test Artist",
            Album = "Test Album",
            Duration = TimeSpan.FromMinutes(3)
        };
    }
}

#region Test Doubles

/// <summary>
/// Test double for MediaType enum.
/// </summary>
public enum TestMediaType
{
    Audio,
    Video
}

/// <summary>
/// Test double for MediaItem model.
/// </summary>
public class TestMediaItem
{
    public string Title { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public TestMediaType MediaType { get; set; }
    public bool IsFavorite { get; set; }
    public int PlayCount { get; set; }
    public DateTime? LastPlayedDate { get; set; }
}

/// <summary>
/// Test double for LibraryService.
/// </summary>
public class TestLibraryService
{
    public List<TestMediaItem> MediaItems { get; } = new();
    public List<TestMediaItem> Songs { get; } = new();
    public List<TestMediaItem> Videos { get; } = new();

    public Task AddItemsAsync(IEnumerable<TestMediaItem> items)
    {
        foreach (var item in items)
        {
            if (!MediaItems.Any(m => m.FilePath == item.FilePath))
            {
                MediaItems.Add(item);
            }
        }
        RefreshCollections();
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(TestMediaItem item)
    {
        MediaItems.Remove(item);
        RefreshCollections();
        return Task.CompletedTask;
    }

    public Task ClearLibraryAsync()
    {
        MediaItems.Clear();
        RefreshCollections();
        return Task.CompletedTask;
    }

    public Task ToggleFavoriteAsync(TestMediaItem item)
    {
        item.IsFavorite = !item.IsFavorite;
        return Task.CompletedTask;
    }

    public Task RecordPlayAsync(TestMediaItem item)
    {
        item.PlayCount++;
        item.LastPlayedDate = DateTime.Now;
        return Task.CompletedTask;
    }

    public void RefreshCollections()
    {
        Songs.Clear();
        Videos.Clear();
        Songs.AddRange(MediaItems.Where(m => m.MediaType == TestMediaType.Audio));
        Videos.AddRange(MediaItems.Where(m => m.MediaType == TestMediaType.Video));
    }
}

#endregion
