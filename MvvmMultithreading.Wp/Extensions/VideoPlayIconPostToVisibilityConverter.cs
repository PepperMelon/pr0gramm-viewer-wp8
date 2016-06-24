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
    using Model;

    public class VideoPlayIconPostToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var post = value as Post;
            if (post.Image.EndsWith(".webm") || post.Image.EndsWith(".gif"))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class VideoPlayIconPostToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var post = value as Post;
            if (post.Image.EndsWith(".webm") || post.Image.EndsWith(".gif"))
            {
                return post.Thumb;
            }

            return post.Image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
