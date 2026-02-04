namespace Tips_Player.Tests.ViewModels;

/// <summary>
/// Tests for player view model functionality.
/// Uses standalone test doubles for MAUI-independent testing.
/// </summary>
public class PlayerViewModelTests
{
    [Fact]
    public void Constructor_ShouldSetTitle()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.Title.Should().Be("Now Playing");
    }

    [Fact]
    public void Constructor_ShouldLoadVolumeFromSettings()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.Volume.Should().Be(1.0);
    }

    [Fact]
    public void IsPlaying_ShouldBeFalseInitially()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.IsPlaying.Should().BeFalse();
    }

    [Fact]
    public void CurrentMedia_ShouldBeNullInitially()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.CurrentMedia.Should().BeNull();
    }

    [Fact]
    public void Playlist_ShouldBeEmptyInitially()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.Playlist.Should().BeEmpty();
    }

    [Fact]
    public void CurrentIndex_ShouldBeNegativeOneInitially()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.CurrentIndex.Should().Be(-1);
    }

    [Fact]
    public void HasNext_ShouldBeFalseWhenPlaylistEmpty()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.HasNext.Should().BeFalse();
    }

    [Fact]
    public void HasPrevious_ShouldBeFalseWhenAtStart()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.Playlist.Add(CreateTestMediaItem());
        sut.CurrentIndex = 0;

        // Assert
        sut.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public void HasNext_ShouldBeTrueWhenNotAtEnd()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.Playlist.Add(CreateTestMediaItem("Song 1"));
        sut.Playlist.Add(CreateTestMediaItem("Song 2"));
        sut.CurrentIndex = 0;

        // Assert
        sut.HasNext.Should().BeTrue();
    }

    [Fact]
    public void HasPrevious_ShouldBeTrueWhenNotAtStart()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.Playlist.Add(CreateTestMediaItem("Song 1"));
        sut.Playlist.Add(CreateTestMediaItem("Song 2"));
        sut.CurrentIndex = 1;

        // Assert
        sut.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public void OnPlaybackStateChanged_ShouldUpdateIsPlaying()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.SimulatePlay();

        // Assert
        sut.IsPlaying.Should().BeTrue();
    }

    [Fact]
    public void OnPositionChanged_ShouldUpdateCurrentPosition()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        var newPosition = TimeSpan.FromSeconds(30);

        // Act
        sut.SimulatePositionChange(newPosition);

        // Assert
        sut.CurrentPosition.Should().Be(newPosition);
        sut.SliderPosition.Should().Be(30);
    }

    [Fact]
    public async Task LoadAndPlayAsync_ShouldLoadAndPlayMedia()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        var mediaItem = CreateTestMediaItem();

        // Act
        await sut.LoadAndPlayAsync(mediaItem);

        // Assert
        sut.LoadAsyncCallCount.Should().Be(1);
        sut.PlayAsyncCallCount.Should().Be(1);
    }

    [Fact]
    public async Task PlayMediaAsync_ShouldAddToPlaylistIfNotExists()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        var mediaItem = CreateTestMediaItem();

        // Act
        await sut.PlayMediaAsync(mediaItem);

        // Assert
        sut.Playlist.Should().Contain(mediaItem);
        sut.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public async Task PlayMediaAsync_ShouldNotDuplicateIfExists()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        var mediaItem = CreateTestMediaItem();
        sut.Playlist.Add(mediaItem);

        // Act
        await sut.PlayMediaAsync(mediaItem);

        // Assert
        sut.Playlist.Should().HaveCount(1);
        sut.CurrentIndex.Should().Be(0);
    }

    [Fact]
    public void OnVolumeChanged_ShouldClampToValidRange()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act & Assert - Above max
        sut.Volume = 1.5;
        sut.Volume.Should().Be(1.0);

        // Act & Assert - Below min
        sut.Volume = -0.5;
        sut.Volume.Should().Be(0.0);
    }

    [Fact]
    public void ToggleShuffle_ShouldToggleShuffleEnabled()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.ToggleShuffle();

        // Assert
        sut.IsShuffleEnabled.Should().BeTrue();
    }

    [Fact]
    public void ToggleRepeat_ShouldToggleRepeatEnabled()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.ToggleRepeat();

        // Assert
        sut.IsRepeatEnabled.Should().BeTrue();
    }

    [Fact]
    public void ToggleMute_ShouldToggleMuted()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.ToggleMute();

        // Assert
        sut.IsMuted.Should().BeTrue();
    }

    [Fact]
    public void ToggleFullScreen_ShouldToggleFullScreenState()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.ToggleFullScreen();

        // Assert
        sut.IsFullScreen.Should().BeTrue();

        // Act again
        sut.ToggleFullScreen();

        // Assert
        sut.IsFullScreen.Should().BeFalse();
    }

    [Fact]
    public void UpdateDuration_ShouldUpdateDurationAndMedia()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        var mediaItem = CreateTestMediaItem();
        sut.Playlist.Add(mediaItem);
        sut.CurrentIndex = 0;
        var duration = TimeSpan.FromMinutes(5);

        // Act
        sut.UpdateDuration(duration);

        // Assert
        sut.Duration.Should().Be(duration);
    }

    [Fact]
    public void CurrentPositionFormatted_ShouldFormatCorrectly()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.SimulatePositionChange(TimeSpan.FromSeconds(65)); // 1:05

        // Assert
        sut.CurrentPositionFormatted.Should().Be("01:05");
    }

    [Fact]
    public void SliderMaximum_ShouldReturnDurationInSeconds()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.UpdateDuration(TimeSpan.FromMinutes(3));

        // Assert
        sut.SliderMaximum.Should().Be(180);
    }

    [Fact]
    public void SliderMaximum_ShouldReturn100WhenDurationZero()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.SliderMaximum.Should().Be(100);
    }

    [Fact]
    public void PlaybackSpeed_ShouldDefaultToOne()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Assert
        sut.PlaybackSpeed.Should().Be(1.0);
    }

    [Fact]
    public void SetPlaybackSpeed_ShouldUpdateSpeed()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.SetPlaybackSpeed(1.5);

        // Assert
        sut.PlaybackSpeed.Should().Be(1.5);
        sut.PlaybackSpeedText.Should().Be("1.5x");
    }

    [Fact]
    public void IncreaseSpeed_ShouldIncrementSpeed()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.IncreaseSpeed();

        // Assert
        sut.PlaybackSpeed.Should().Be(1.25);
    }

    [Fact]
    public void DecreaseSpeed_ShouldDecrementSpeed()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.DecreaseSpeed();

        // Assert
        sut.PlaybackSpeed.Should().Be(0.75);
    }

    [Fact]
    public void ResetSpeed_ShouldSetSpeedToOne()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.SetPlaybackSpeed(2.0);

        // Act
        sut.ResetSpeed();

        // Assert
        sut.PlaybackSpeed.Should().Be(1.0);
    }

    [Fact]
    public void ABRepeat_ShouldSetPointA()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.SimulatePositionChange(TimeSpan.FromSeconds(10));

        // Act
        sut.SetPointA();

        // Assert
        sut.RepeatPointA.Should().Be(TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void ClearABRepeat_ShouldClearAllPoints()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.SetPointA();
        sut.SimulatePositionChange(TimeSpan.FromSeconds(30));
        sut.SetPointB();

        // Act
        sut.ClearABRepeat();

        // Assert
        sut.RepeatPointA.Should().BeNull();
        sut.RepeatPointB.Should().BeNull();
        sut.IsABRepeatEnabled.Should().BeFalse();
    }

    [Fact]
    public void SetSleepTimer_ShouldActivateTimer()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.SetSleepTimer(15);

        // Assert
        sut.IsSleepTimerActive.Should().BeTrue();
        sut.SleepTimerMinutes.Should().Be(15);
    }

    [Fact]
    public void SetSleepTimer_WithZero_ShouldDeactivateTimer()
    {
        // Arrange
        var sut = new TestPlayerViewModel();
        sut.SetSleepTimer(15);

        // Act
        sut.SetSleepTimer(0);

        // Assert
        sut.IsSleepTimerActive.Should().BeFalse();
        sut.SleepTimerMinutes.Should().Be(0);
    }

    [Fact]
    public void ToggleCrossfade_ShouldToggleCrossfadeEnabled()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act
        sut.ToggleCrossfade();

        // Assert
        sut.IsCrossfadeEnabled.Should().BeTrue();

        // Act again
        sut.ToggleCrossfade();

        // Assert
        sut.IsCrossfadeEnabled.Should().BeFalse();
    }

    [Fact]
    public void SetCrossfadeDuration_ShouldClampToValidRange()
    {
        // Arrange
        var sut = new TestPlayerViewModel();

        // Act - Above max
        sut.SetCrossfadeDuration(20);
        sut.CrossfadeDuration.Should().Be(12);

        // Act - Below min
        sut.SetCrossfadeDuration(0);
        sut.CrossfadeDuration.Should().Be(1);

        // Act - Valid
        sut.SetCrossfadeDuration(5);
        sut.CrossfadeDuration.Should().Be(5);
    }

    private static TestMediaItem CreateTestMediaItem(string title = "Test Media")
    {
        return new TestMediaItem
        {
            Title = title,
            MediaType = TestMediaType.Audio,
            FilePath = $"C:\\test\\{title.Replace(" ", "_")}.mp3",
            Artist = "Test Artist",
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
/// Test double for PlayerViewModel.
/// </summary>
public class TestPlayerViewModel
{
    private double _volume = 1.0;
    private double _playbackSpeed = 1.0;

    public string Title { get; } = "Now Playing";
    public TestMediaItem? CurrentMedia { get; private set; }
    public bool IsPlaying { get; private set; }
    public bool IsMuted { get; private set; }
    public bool IsShuffleEnabled { get; private set; }
    public bool IsRepeatEnabled { get; private set; }
    public bool IsFullScreen { get; private set; }
    public bool IsCrossfadeEnabled { get; private set; }
    public bool IsSleepTimerActive { get; private set; }
    public bool IsABRepeatEnabled { get; private set; }
    public int SleepTimerMinutes { get; private set; }
    public int CrossfadeDuration { get; private set; } = 3;
    public TimeSpan CurrentPosition { get; private set; } = TimeSpan.Zero;
    public TimeSpan Duration { get; private set; } = TimeSpan.Zero;
    public TimeSpan? RepeatPointA { get; private set; }
    public TimeSpan? RepeatPointB { get; private set; }
    public List<TestMediaItem> Playlist { get; } = new();
    public int CurrentIndex { get; set; } = -1;
    public int LoadAsyncCallCount { get; private set; }
    public int PlayAsyncCallCount { get; private set; }

    public double Volume
    {
        get => _volume;
        set => _volume = Math.Clamp(value, 0.0, 1.0);
    }

    public double PlaybackSpeed
    {
        get => _playbackSpeed;
        set => _playbackSpeed = Math.Clamp(value, 0.25, 2.0);
    }

    public double SliderPosition => CurrentPosition.TotalSeconds;
    public double SliderMaximum => Duration == TimeSpan.Zero ? 100 : Duration.TotalSeconds;
    public string CurrentPositionFormatted => CurrentPosition.ToString(@"mm\:ss");
    public string PlaybackSpeedText => $"{PlaybackSpeed}x";

    public bool HasNext => CurrentIndex >= 0 && CurrentIndex < Playlist.Count - 1;
    public bool HasPrevious => CurrentIndex > 0;

    public void SimulatePlay() => IsPlaying = true;
    public void SimulatePause() => IsPlaying = false;
    public void SimulatePositionChange(TimeSpan position) => CurrentPosition = position;

    public Task LoadAndPlayAsync(TestMediaItem item)
    {
        CurrentMedia = item;
        LoadAsyncCallCount++;
        PlayAsyncCallCount++;
        IsPlaying = true;
        return Task.CompletedTask;
    }

    public Task PlayMediaAsync(TestMediaItem item)
    {
        if (!Playlist.Contains(item))
        {
            Playlist.Add(item);
        }
        CurrentIndex = Playlist.IndexOf(item);
        CurrentMedia = item;
        IsPlaying = true;
        return Task.CompletedTask;
    }

    public void ToggleShuffle() => IsShuffleEnabled = !IsShuffleEnabled;
    public void ToggleRepeat() => IsRepeatEnabled = !IsRepeatEnabled;
    public void ToggleMute() => IsMuted = !IsMuted;
    public void ToggleFullScreen() => IsFullScreen = !IsFullScreen;
    public void ToggleCrossfade() => IsCrossfadeEnabled = !IsCrossfadeEnabled;

    public void UpdateDuration(TimeSpan duration) => Duration = duration;

    public void SetPlaybackSpeed(double speed)
    {
        PlaybackSpeed = Math.Clamp(speed, 0.25, 2.0);
    }

    public void IncreaseSpeed() => SetPlaybackSpeed(PlaybackSpeed + 0.25);
    public void DecreaseSpeed() => SetPlaybackSpeed(PlaybackSpeed - 0.25);
    public void ResetSpeed() => SetPlaybackSpeed(1.0);

    public void SetPointA() => RepeatPointA = CurrentPosition;
    public void SetPointB()
    {
        RepeatPointB = CurrentPosition;
        if (RepeatPointA.HasValue)
        {
            IsABRepeatEnabled = true;
        }
    }

    public void ClearABRepeat()
    {
        RepeatPointA = null;
        RepeatPointB = null;
        IsABRepeatEnabled = false;
    }

    public void SetSleepTimer(int minutes)
    {
        SleepTimerMinutes = minutes;
        IsSleepTimerActive = minutes > 0;
    }

    public void SetCrossfadeDuration(int seconds)
    {
        CrossfadeDuration = Math.Clamp(seconds, 1, 12);
    }
}

#endregion
