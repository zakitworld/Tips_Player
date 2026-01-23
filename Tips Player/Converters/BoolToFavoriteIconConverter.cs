using System.Globalization;
using Tips_Player.Helpers;

namespace Tips_Player.Converters;

public class BoolToFavoriteIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool isFavorite)
        {
            return FontIcons.HeartFilled;
        }
        return FontIcons.HeartFilled;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
