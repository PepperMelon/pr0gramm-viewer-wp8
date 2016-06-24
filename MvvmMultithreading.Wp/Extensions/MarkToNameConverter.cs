using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;
    using Resources;

    public class MarkToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mark = value is int ? (int) value : 0;
            if (mark == 1)
            {
                return AppResources.Newfag;
            }
            else if (mark == 0)
            {
                return AppResources.Fag;
            }
            else if (mark == 2)
            {
                return AppResources.Oldfag;
            }
            else if (mark == 3)
            {
                return AppResources.Admin;
            }
            else if (mark == 4)
            {
                return AppResources.Closed;
            }
            else if (mark == 5)
            {
                return AppResources.Mod;
            }
            else if (mark == 6)
            {
                return AppResources.TileTableOwner;
            }
            else if (mark == 7)
            {
                return AppResources.LivingLegend;
            }
            else if (mark == 8)
            {
                return AppResources.ProWichtler;
            }
            else if (mark == 9)
            {
                return AppResources.Donator;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
