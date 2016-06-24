using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Constants
{
    using Extensions;
    using Resources;
    using ViewModel;

    public static class ApiCalls
    {
        public const string TopShareUrl = "http://pr0gramm.com/top/{0}";
        public const string NewShareUrl = "http://pr0gramm.com/new/{0}";

        public const string ImageUrl = "http://img.pr0gramm.com/";
        public const string ThumbUrl = "http://thumb.pr0gramm.com/";
        public const string HotPostsUrl = "http://pr0gramm.com/api/items/get?flags={0}";
        public const string PostInfoUrl = "http://pr0gramm.com/api/items/info?itemId=";
        public const string AuthUrl = "http://pr0gramm.com/api/user/login";
        public const string CommentUrl = "http://pr0gramm.com/api/comments/post";
        public const string CommentVoteUrl = "http://pr0gramm.com/api/comments/vote";
        public const string PostVoteUrl = "http://pr0gramm.com/api/items/vote";
        public const string AddTagsUrl = "http://pr0gramm.com/api/tags/add";
        public const string VoteTagUrl = "http://pr0gramm.com/api/tags/vote";
        public const string UploadPicFromUrlUrl = "http://pr0gramm.com/api/items/post";
        public const string SyncVotesUrl = "http://pr0gramm.com/api/user/sync?lastId={0}";
        public const string UserUrl = "http://pr0gramm.com/api/profile/info?name={0}";
        public const string SelfUserLikes = "http://pr0gramm.com/api/items/get?likes={0}&flags={1}&self=true";
        public const string SelfUserUploads = "http://pr0gramm.com/api/profile/info?name={0}&flags={1}";
        public const string CommonPostInfoUrl = "http://pr0gramm.com/api/items/get?id={0}&flags=7";
        public const string UserUploads = "http://pr0gramm.com/api/items/get?flags={1}&user={0}";

        public static string HotPosts(int flags, string tag)
        {
            var sb = new StringBuilder();
            if (ViewModelLocator.Main.Category == Category.Hot)
            {
                sb.AppendFormat("{0}&promoted=1", string.Format(HotPostsUrl, flags));
            }
            else
            {
                sb.AppendFormat("{0}", string.Format(HotPostsUrl, flags));
            }

            if (!string.IsNullOrEmpty(tag))
            {
                sb.AppendFormat("&tags={0}", tag);
            }

            return sb.ToString();
        }

        public static string HotPostsWithOlderParameter(int promotedId, int flags, string tag)
        {
            return string.Format("{0}&older={1}", HotPosts(flags, tag), promotedId);
        }

        public static string PostInfo(int postId)
        {
            return string.Format("{0}{1}", PostInfoUrl, postId);
        }
    }

    public enum PostFilter
    {
        SFW = 0,
        NSFW = 1,
        NSFL = 2
    }

    public enum FagFilter
    {
        [LocalizedDescription("All", typeof(AppResources))]
        Alle,

        [LocalizedDescription("Newfag", typeof(AppResources))]
        Neuschwuchtel,

        [LocalizedDescription("Fag", typeof(AppResources))]
        Schwuchtel
    }
}