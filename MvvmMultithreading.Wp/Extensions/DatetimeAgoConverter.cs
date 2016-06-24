namespace Pr0gramm.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;
    using Resources;

    public class DatetimeAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = value is DateTime ? (DateTime) value : new DateTime();
            var dateDiff = DateTime.Now - date;

            var sb = new StringBuilder();
            if (dateDiff.Days > 0)
            {
                sb.AppendFormat("{0}d ", dateDiff.Days);
            }
            if (dateDiff.Hours > 0 && dateDiff.Days < 8)
            {
                sb.AppendFormat("{0}h ", dateDiff.Hours);
            }
            if (dateDiff.Minutes > 0 && dateDiff.Days < 1)
            {
                sb.AppendFormat("{0}m ", dateDiff.Minutes);
            }

            if (dateDiff.TotalMinutes < 1)
            {
                sb.AppendFormat("{0}  ", AppResources.Now);
            }
            else
            {
                sb = new StringBuilder(string.Format(AppResources.Ago, sb));
            }

            return sb.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
