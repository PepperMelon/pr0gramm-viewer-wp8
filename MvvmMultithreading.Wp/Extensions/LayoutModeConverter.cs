using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using Constants;
    using Microsoft.Phone.Controls;
    using View;
    using ViewModel;

    public class LayoutModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ViewModelLocator.Settings.PostView == PostView.Bilder.ToString())
            {
                return LongListSelectorLayoutMode.Grid;
            }

            if (ViewModelLocator.Settings.PostView == PostView.Effizient.ToString()|| ViewModelLocator.Settings.PostView == PostView.BilderOhneRahmen.ToString())
            {
                return LongListSelectorLayoutMode.Grid;
            }

            return LongListSelectorLayoutMode.List;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GridCellSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (ViewModelLocator.Settings.PostView == PostView.Bilder.ToString() || ViewModelLocator.Settings.PostView == PostView.BilderOhneRahmen.ToString())
            {
                return new Size(110, 110);
            }

            if (ViewModelLocator.Settings.PostView == PostView.Effizient.ToString())
            {
                return new Size(220, 220);
            }

            return new Size(110, 110);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
