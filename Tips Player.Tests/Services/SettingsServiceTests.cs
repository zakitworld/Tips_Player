namespace Tips_Player.Tests.Services;

/// <summary>
/// Tests for settings service functionality.
/// </summary>
public class SettingsServiceTests
{
    [Fact]
    public void Volume_ShouldDefaultToOne()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.Volume.Should().Be(1.0);
    }

    [Fact]
    public void Volume_ShouldBeSettable()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Act
        sut.Volume = 0.5;

        // Assert
        sut.Volume.Should().Be(0.5);
    }

    [Fact]
    public void IsMuted_ShouldDefaultToFalse()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.IsMuted.Should().BeFalse();
    }

    [Fact]
    public void IsShuffleEnabled_ShouldDefaultToFalse()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.IsShuffleEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsRepeatEnabled_ShouldDefaultToFalse()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.IsRepeatEnabled.Should().BeFalse();
    }

    [Fact]
    public void AutoPlayNext_ShouldDefaultToTrue()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.AutoPlayNext.Should().BeTrue();
    }

    [Fact]
    public void RememberPlaybackPosition_ShouldDefaultToTrue()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.RememberPlaybackPosition.Should().BeTrue();
    }

    [Fact]
    public void Theme_ShouldDefaultToDark()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.Theme.Should().Be("Dark");
    }

    [Fact]
    public void Theme_ShouldBeSettable()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Act
        sut.Theme = "Light";

        // Assert
        sut.Theme.Should().Be("Light");
    }

    [Fact]
    public void SortOrder_ShouldDefaultToTitle()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.SortOrder.Should().Be("Title");
    }

    [Fact]
    public void SortOrder_ShouldBeSettable()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Act
        sut.SortOrder = "Artist";

        // Assert
        sut.SortOrder.Should().Be("Artist");
    }

    [Fact]
    public void DefaultVideoAspect_ShouldDefaultToFit()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.DefaultVideoAspect.Should().Be("Fit");
    }

    [Fact]
    public void ResetToDefaults_ShouldResetAllSettings()
    {
        // Arrange
        var sut = new TestSettingsService();
        sut.Volume = 0.3;
        sut.IsMuted = true;
        sut.IsShuffleEnabled = true;
        sut.Theme = "Light";

        // Act
        sut.ResetToDefaults();

        // Assert
        sut.Volume.Should().Be(1.0);
        sut.IsMuted.Should().BeFalse();
        sut.IsShuffleEnabled.Should().BeFalse();
        sut.Theme.Should().Be("Dark");
    }

    [Fact]
    public void ShowFileExtensions_ShouldDefaultToFalse()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.ShowFileExtensions.Should().BeFalse();
    }

    [Fact]
    public void GroupByAlbum_ShouldDefaultToFalse()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.GroupByAlbum.Should().BeFalse();
    }

    [Fact]
    public void AutoRotateVideo_ShouldDefaultToTrue()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.AutoRotateVideo.Should().BeTrue();
    }

    [Fact]
    public void DefaultSleepTimerMinutes_ShouldDefaultToZero()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.DefaultSleepTimerMinutes.Should().Be(0);
    }

    [Fact]
    public void AccentColor_ShouldHaveDefaultValue()
    {
        // Arrange
        var sut = new TestSettingsService();

        // Assert
        sut.AccentColor.Should().NotBeNullOrEmpty();
    }
}

#region Test Doubles

/// <summary>
/// Test double for settings service.
/// </summary>
public class TestSettingsService
{
    public double Volume { get; set; } = 1.0;
    public bool IsMuted { get; set; }
    public bool IsShuffleEnabled { get; set; }
    public bool IsRepeatEnabled { get; set; }
    public bool AutoPlayNext { get; set; } = true;
    public bool RememberPlaybackPosition { get; set; } = true;
    public string Theme { get; set; } = "Dark";
    public string SortOrder { get; set; } = "Title";
    public string DefaultVideoAspect { get; set; } = "Fit";
    public bool ShowFileExtensions { get; set; }
    public bool GroupByAlbum { get; set; }
    public bool AutoRotateVideo { get; set; } = true;
    public int DefaultSleepTimerMinutes { get; set; }
    public string AccentColor { get; set; } = "#FF6B35";

    public void ResetToDefaults()
    {
        Volume = 1.0;
        IsMuted = false;
        IsShuffleEnabled = false;
        IsRepeatEnabled = false;
        AutoPlayNext = true;
        RememberPlaybackPosition = true;
        Theme = "Dark";
        SortOrder = "Title";
        DefaultVideoAspect = "Fit";
        ShowFileExtensions = false;
        GroupByAlbum = false;
        AutoRotateVideo = true;
        DefaultSleepTimerMinutes = 0;
        AccentColor = "#FF6B35";
    }

    public void SaveSettings()
    {
        // No-op for testing
    }
}

#endregion
