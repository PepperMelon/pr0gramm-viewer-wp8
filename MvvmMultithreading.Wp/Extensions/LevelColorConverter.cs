namespace Pr0gramm.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;
    using System.Windows.Media;
    using Model;

    public class LevelColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var item = value as Comment;
                var level = item.Level;

                switch (level)
                {
                    case 1:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                    case 2:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 140, 0));
                    case 3:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 85, 0));
                    case 4:
                        return new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    case 5:
                        return new SolidColorBrush(Color.FromArgb(255, 139, 0, 0));
                    case 6:
                        return new SolidColorBrush(Color.FromArgb(255, 30, 144, 255));
                    case 7:
                        return new SolidColorBrush(Color.FromArgb(255, 0, 0, 255));
                    case 8:
                        return new SolidColorBrush(Color.FromArgb(255, 0, 0, 139));
                    case 9:
                        return new SolidColorBrush(Color.FromArgb(255, 128, 128, 128));
                }

                if (level > 9 && level < 16)
                {
                    var bDifference = (byte) (250 - (level - 9 * 50));
                    return new SolidColorBrush(Color.FromArgb(255, 128, 255, bDifference)); 
                }
                if (level > 15 && level < 21)
                {
                    var bDifference = (byte)(255 - (level - 15 * 50));
                    return new SolidColorBrush(Color.FromArgb(255, 128, bDifference, 255));
                }

                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
            catch (Exception)
            {
                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
