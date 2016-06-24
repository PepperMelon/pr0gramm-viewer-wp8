namespace Pr0gramm.Extensions
{
    using System;
    using System.Globalization;
    using System.Text;
    using System.Windows.Data;
    using Model;
    using Resources;

    public class UpDownVotePointCoverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var comment = value as Comment;
            if (comment != null)
            {
                return PointsToString(comment.Up, comment.Down);
            }

            var post = value as Post;
            if (post != null)
            {
                return PointsToString(post.Up, post.Down);
            }

            return AppResources.Loading;
        }

        private static object PointsToString(int upVotes, int downVotes)
        {
            var points = upVotes - downVotes;
            if (points == 1 || points == -1)
            {
                return string.Format("{0} {1}", points, AppResources.Point);
            }

            return string.Format("{0} {1}", points, AppResources.Points);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
