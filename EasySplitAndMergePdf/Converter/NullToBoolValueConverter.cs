using System;
using System.Windows.Data;
using System.Globalization;

namespace EasySplitAndMergePdf.Converter
{
    /// <summary>
    /// Converts null to boolean. 
    /// </summary>
    /// <remarks>
    /// If parameter is not specified or specified with null:
    /// if passed value is null then true is returned, otherwise false. 
    /// If parameter is specified then inversion is done:
    /// if passed value is null then false is returned, otherwise true.
    /// </remarks>
    [ValueConversion(typeof(object), typeof(bool))]
    public class NullToBoolValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter == null ? value == null : !(value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
