using System.Globalization;

namespace Tips_Player.Converters;

public class BoolToFavoriteColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isFavorite)
        {
            return isFavorite ? Color.FromArgb("#EF4444") : Color.FromArgb("#A1A1AA");
        }
        return Color.FromArgb("#A1A1AA");
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
