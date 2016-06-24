using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Model
{
    using System.Collections.ObjectModel;
    using BaseClasses;
    using GalaSoft.MvvmLight.Command;
    using Logic;
    using Newtonsoft.Json;

    public class UserProfile : ExtendedViewModelBase
    {
        private ObservableCollection<Comment> comments;
        private ObservableCollection<Post> likes = new ObservableCollection<Post>();
        private ObservableCollection<Post> uploads = new ObservableCollection<Post>();

        public User User { get; set; }

        public ObservableCollection<Comment> Comments
        {
            get
            {
                return comments;
            }
            set
            {
                comments = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Post> PostLikes
        {
            get
            {
                return likes;
            }
            set
            {
                likes = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Post> PostUploads
        {
            get
            {
                return uploads;
            }
            set
            {
                uploads = value;
                this.OnPropertyChanged();
            }
        }

        public int CommentCount { get; set; }

        public int UploadCount { get; set; }

        public int LikeCount { get; set; }

        public int TagCount { get; set; }

        public Action LoadLikePosts { get; set; }

        public Action LoadUploadPosts { get; set; }

        public RelayCommand LoadLikePostsCommand
        {
            get
            {
                return new RelayCommand(() => LoadLikePosts());
            }
        }

        public RelayCommand LoadUploadsPostsCommand
        {
            get
            {
                return new RelayCommand(() => LoadUploadPosts());
            }
        }

        public static UserProfile FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UserProfile>(json);
        }
    }

    public class User
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Score { get; set; }

        public int Mark { get; set; }

        public int Admin { get; set; }

        public int Banned { get; set; }
    }

    public class DefaultObject
    {
        public int Id { get; set; }

        public string Thumb { get; set; }

    }
}
