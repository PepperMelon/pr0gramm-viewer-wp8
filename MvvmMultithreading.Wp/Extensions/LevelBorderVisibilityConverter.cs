namespace Pr0gramm.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using Model;
    using Resources;

    public class LevelBorderVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var item = value as Comment;
                var level = item.Level;

                if (level == 0 || level == null)
                {
                    return 0;
                }

                return 1;
            }
            catch (Exception)
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
