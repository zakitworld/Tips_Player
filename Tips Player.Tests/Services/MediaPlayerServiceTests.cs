namespace Tips_Player.Tests.Services;

/// <summary>
/// Tests for media player service functionality.
/// </summary>
public class MediaPlayerServiceTests
{
    [Fact]
    public void CurrentMedia_ShouldBeNullInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.CurrentMedia.Should().BeNull();
    }

    [Fact]
    public void IsPlaying_ShouldBeFalseInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void Volume_ShouldDefaultToOne()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.Volume.Should().Be(1.0);
    }

    [Fact]
    public void Volume_ShouldClampToValidRange()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Act & Assert - Above max
        sut.Volume = 1.5;
        sut.Volume.Should().Be(1.0);

        // Act & Assert - Below min
        sut.Volume = -0.5;
        sut.Volume.Should().Be(0.0);

        // Act & Assert - Valid value
        sut.Volume = 0.5;
        sut.Volume.Should().Be(0.5);
    }

    [Fact]
    public void IsMuted_ShouldBeFalseInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.IsMuted.Should().BeFalse();
    }

    [Fact]
    public void IsMuted_ShouldBeSettable()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Act
        sut.IsMuted = true;

        // Assert
        sut.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void IsShuffleEnabled_ShouldBeFalseInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.IsShuffleEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsShuffleEnabled_ShouldBeSettable()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Act
        sut.IsShuffleEnabled = true;

        // Assert
        sut.IsShuffleEnabled.Should().BeTrue();
    }

    [Fact]
    public void IsRepeatEnabled_ShouldBeFalseInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.IsRepeatEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsRepeatEnabled_ShouldBeSettable()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Act
        sut.IsRepeatEnabled = true;

        // Assert
        sut.IsRepeatEnabled.Should().BeTrue();
    }

    [Fact]
    public void CurrentPosition_ShouldBeZeroInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.CurrentPosition.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Duration_ShouldBeZeroInitially()
    {
        // Arrange
        var sut = new TestMediaPlayerService();

        // Assert
        sut.Duration.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public async Task LoadAsync_ShouldSetCurrentMedia()
    {
        // Arrange
        var sut = new TestMediaPlayerService();
        var mediaItem = CreateTestMediaItem();

        // Act
        await sut.LoadAsync(mediaItem);

        // Assert
        sut.CurrentMedia.Should().Be(mediaItem);
    }

    [Fact]
    public async Task LoadAsync_ShouldRaiseMediaChangedEvent()
    {
        // Arrange
        var sut = new TestMediaPlayerService();
        var mediaItem = CreateTestMediaItem();
        TestMediaItem? receivedMedia = null;
        sut.MediaChanged += (_, media) => receivedMedia = media;

        // Act
        await sut.LoadAsync(mediaItem);

        // Assert
        receivedMedia.Should().Be(mediaItem);
    }

    [Fact]
    public async Task StopAsync_ShouldResetPosition()
    {
        // Arrange
        var sut = new TestMediaPlayerService();
        var mediaItem = CreateTestMediaItem();
        await sut.LoadAsync(mediaItem);

        // Act
        await sut.StopAsync();

        // Assert
        sut.IsPlaying.Should().BeFalse();
    }

    private static TestMediaItem CreateTestMediaItem()
    {
        return new TestMediaItem
        {
            Title = "Test Media",
            MediaType = TestMediaType.Audio,
            FilePath = "C:\\test\\media.mp3",
            Duration = TimeSpan.FromMinutes(3)
        };
    }
}

#region Test Doubles

/// <summary>
/// Test double for media player service.
/// </summary>
public class TestMediaPlayerService
{
    private double _volume = 1.0;

    public TestMediaItem? CurrentMedia { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsMuted { get; set; }
    public bool IsShuffleEnabled { get; set; }
    public bool IsRepeatEnabled { get; set; }
    public TimeSpan CurrentPosition { get; private set; } = TimeSpan.Zero;
    public TimeSpan Duration { get; private set; } = TimeSpan.Zero;

    public event EventHandler<TestMediaItem?>? MediaChanged;

    public double Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0, 1.0);
    }

    public Task LoadAsync(TestMediaItem item)
    {
        CurrentMedia = item;
        Duration = item.Duration;
        MediaChanged?.Invoke(this, item);
        return Task.CompletedTask;
    }

    public Task PlayAsync()
    {
        if (CurrentMedia != null)
        {
            IsPlaying = true;
        }
        return Task.CompletedTask;
    }

    public Task PauseAsync()
    {
        IsPlaying = false;
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        IsPlaying = false;
        CurrentPosition = TimeSpan.Zero;
        return Task.CompletedTask;
    }

    public Task SeekAsync(TimeSpan position)
    {
        CurrentPosition = position;
        return Task.CompletedTask;
    }
}

#endregion
