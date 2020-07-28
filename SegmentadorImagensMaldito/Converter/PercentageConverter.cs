using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace SegmentadorImagensMaldito.Converter
{
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * System.Convert.ToDouble(parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return instance ??= new PercentageConverter();
        }
    }
}
