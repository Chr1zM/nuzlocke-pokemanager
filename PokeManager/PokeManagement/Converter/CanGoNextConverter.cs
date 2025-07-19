using System.Globalization;
using System.Windows.Data;

namespace PokeManager.PokeManagement.Converter
{
    public class CanGoNextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return false;

            if (values[0] is int currentPage &&
                values[1] is int totalPages &&
                values[2] is bool isLoading)
            {
                return currentPage < totalPages && !isLoading;
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }

}
