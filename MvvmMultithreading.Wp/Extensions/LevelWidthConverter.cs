namespace Pr0gramm.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using Model;

    public class LevelWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var item = value as Comment;
                var level = item.Level;

                if (level == 0 || level == null)
                {
                    return 465;
                }

                var width = 465 - (double)level * 10;

                return width;
            }
            catch (Exception)
            {
                return 465;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
