using System.Globalization;

namespace Tips_Player.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isActive && isActive)
        {
            // Return accent color when active
            if (Application.Current?.Resources.TryGetValue("AccentColor", out var accentColor) == true)
            {
                return accentColor;
            }
            return Colors.DodgerBlue;
        }

        // Return surface color when inactive
        if (Application.Current?.Resources.TryGetValue("SurfaceBackground", out var surfaceColor) == true)
        {
            return surfaceColor;
        }
        return Colors.DarkGray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
