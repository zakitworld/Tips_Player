using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tips_Player.Models;
using Tips_Player.Services.Interfaces;

namespace Tips_Player.ViewModels;

public partial class LyricsViewModel : ObservableObject
{
    private readonly ILyricsService _lyricsService;
    private readonly IMediaPlayerService _mediaPlayerService;
    private Timer? _updateTimer;

    [ObservableProperty]
    private Lyrics? _currentLyrics;

    [ObservableProperty]
    private LyricLine? _currentLine;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _hasLyrics;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private MediaItem? _currentMedia;

    [ObservableProperty]
    private bool _showPlainText;

    [ObservableProperty]
    private bool _autoScroll = true;

    [ObservableProperty]
    private int _fontSize = 18;

    public LyricsViewModel(ILyricsService lyricsService, IMediaPlayerService mediaPlayerService)
    {
        _lyricsService = lyricsService;
        _mediaPlayerService = mediaPlayerService;
        _mediaPlayerService.MediaChanged += OnMediaChanged;
    }

    private async void OnMediaChanged(object? sender, MediaItem? media)
    {
        CurrentMedia = media;
        await LoadLyricsAsync();
    }

    public async Task LoadLyricsAsync()
    {
        if (CurrentMedia == null)
        {
            CurrentLyrics = null;
            HasLyrics = false;
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            // Try to get lyrics from LRC file first
            var lrcPath = Path.ChangeExtension(CurrentMedia.FilePath, ".lrc");
            if (File.Exists(lrcPath))
            {
                CurrentLyrics = await _lyricsService.GetLyricsFromFileAsync(lrcPath);
            }
            else
            {
                // Try to fetch from online
                CurrentLyrics = await _lyricsService.GetLyricsAsync(CurrentMedia.Title, CurrentMedia.Artist);
            }

            HasLyrics = CurrentLyrics != null && (CurrentLyrics.Lines.Count > 0 || !string.IsNullOrEmpty(CurrentLyrics.PlainText));

            if (HasLyrics && CurrentLyrics!.IsSynced)
            {
                StartSyncTimer();
            }
            else
            {
                StopSyncTimer();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load lyrics: {ex.Message}";
            HasLyrics = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void StartSyncTimer()
    {
        StopSyncTimer();
        _updateTimer = new Timer(UpdateLyricsPosition, null, 0, 100);
    }

    private void StopSyncTimer()
    {
        _updateTimer?.Dispose();
        _updateTimer = null;
    }

    private void UpdateLyricsPosition(object? state)
    {
        if (CurrentLyrics == null || !CurrentLyrics.IsSynced) return;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            var position = _mediaPlayerService.CurrentPosition;
            CurrentLyrics.UpdateActiveState(position);
            CurrentLine = CurrentLyrics.GetCurrentLine(position);
        });
    }

    [RelayCommand]
    private void ToggleView()
    {
        ShowPlainText = !ShowPlainText;
    }

    [RelayCommand]
    private void IncreaseFontSize()
    {
        if (FontSize < 32)
            FontSize += 2;
    }

    [RelayCommand]
    private void DecreaseFontSize()
    {
        if (FontSize > 12)
            FontSize -= 2;
    }

    [RelayCommand]
    private void ToggleAutoScroll()
    {
        AutoScroll = !AutoScroll;
    }

    [RelayCommand]
    private async Task SeekToLineAsync(LyricLine? line)
    {
        if (line == null) return;
        await _mediaPlayerService.SeekAsync(line.Timestamp);
    }

    public void OnDisappearing()
    {
        StopSyncTimer();
    }

    public void OnAppearing()
    {
        if (CurrentLyrics?.IsSynced == true)
        {
            StartSyncTimer();
        }
    }
}
