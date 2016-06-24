namespace Pr0gramm.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using Model;

    public class LevelMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var item = value as Comment;
                var level = item.Level;

                if (level == 0 || level == null)
                {
                    return new Thickness(0, 10, 5, 5);
                }

                var left = (double)(level * 10);
                return new Thickness(left, 10, 0, 5);
            }
            catch (Exception)
            {
                return new Thickness(0, 10, 5, 5);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
