using CommunityToolkit.Mvvm.ComponentModel;

namespace Tips_Player.Models;

public partial class LyricLine : ObservableObject
{
    [ObservableProperty]
    private TimeSpan _timestamp;

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _isActive;

    [ObservableProperty]
    private bool _isPast;
}

public partial class Lyrics : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _artist = string.Empty;

    [ObservableProperty]
    private string _source = string.Empty;

    [ObservableProperty]
    private bool _isSynced;

    [ObservableProperty]
    private List<LyricLine> _lines = [];

    [ObservableProperty]
    private string _plainText = string.Empty;

    public static Lyrics Parse(string lrcContent)
    {
        var lyrics = new Lyrics { IsSynced = true };
        var lines = new List<LyricLine>();

        foreach (var line in lrcContent.Split('\n'))
        {
            var trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed)) continue;

            // Parse metadata
            if (trimmed.StartsWith("[ti:"))
                lyrics.Title = trimmed[4..^1];
            else if (trimmed.StartsWith("[ar:"))
                lyrics.Artist = trimmed[4..^1];
            else if (trimmed.StartsWith("["))
            {
                // Parse timestamp [mm:ss.xx] or [mm:ss]
                var closeBracket = trimmed.IndexOf(']');
                if (closeBracket > 0)
                {
                    var timeStr = trimmed[1..closeBracket];
                    var text = trimmed[(closeBracket + 1)..].Trim();

                    if (TryParseTimestamp(timeStr, out var timestamp))
                    {
                        lines.Add(new LyricLine
                        {
                            Timestamp = timestamp,
                            Text = text
                        });
                    }
                }
            }
        }

        lyrics.Lines = lines.OrderBy(l => l.Timestamp).ToList();
        lyrics.PlainText = string.Join("\n", lines.Select(l => l.Text));
        return lyrics;
    }

    private static bool TryParseTimestamp(string timeStr, out TimeSpan timestamp)
    {
        timestamp = TimeSpan.Zero;
        try
        {
            var parts = timeStr.Split(':');
            if (parts.Length >= 2)
            {
                var minutes = int.Parse(parts[0]);
                var secondsParts = parts[1].Split('.');
                var seconds = int.Parse(secondsParts[0]);
                var milliseconds = secondsParts.Length > 1
                    ? int.Parse(secondsParts[1].PadRight(3, '0')[..3])
                    : 0;

                timestamp = new TimeSpan(0, 0, minutes, seconds, milliseconds);
                return true;
            }
        }
        catch { }
        return false;
    }

    public LyricLine? GetCurrentLine(TimeSpan position)
    {
        LyricLine? current = null;
        foreach (var line in Lines)
        {
            if (line.Timestamp <= position)
                current = line;
            else
                break;
        }
        return current;
    }

    public void UpdateActiveState(TimeSpan position)
    {
        var currentLine = GetCurrentLine(position);
        foreach (var line in Lines)
        {
            line.IsActive = line == currentLine;
            line.IsPast = line.Timestamp < position && line != currentLine;
        }
    }
}
