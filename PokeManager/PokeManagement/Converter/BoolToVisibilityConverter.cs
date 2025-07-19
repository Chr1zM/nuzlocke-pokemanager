using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PokeManager.PokeManagement.Converter;

public class BoolToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; } = false;
    public bool CollapseInsteadOfHidden { get; set; } = true;

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return Visibility.Collapsed;

        if (Invert)
            boolValue = !boolValue;

        return boolValue
            ? Visibility.Visible
            : CollapseInsteadOfHidden
                ? Visibility.Collapsed
                : Visibility.Hidden;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Visibility visibility)
            return false;

        return visibility == Visibility.Visible;
    }
}