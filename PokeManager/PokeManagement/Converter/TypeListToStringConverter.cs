using System.Globalization;
using System.Windows.Data;

namespace PokeManager.PokeManagement.Converter;

public class TypeListToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is List<string> types)
            return string.Join(", ", types);

        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}