using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Globalization;
    using System.Windows.Data;

    public class MultiDeviceHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var height = int.Parse(parameter.ToString());

            if (App.Current.Host.Content.ScaleFactor == 100)
            {
                return height;
            }
            else if (App.Current.Host.Content.ScaleFactor == 160)
            {
                return height;
            }
            else if (App.Current.Host.Content.ScaleFactor == 150)
            {
                return height + 53;
            }

            return height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
