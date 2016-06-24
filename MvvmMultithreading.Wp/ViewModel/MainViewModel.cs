namespace Pr0gramm.ViewModel
{
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using BaseClasses;
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Command;
    using GalaSoft.MvvmLight.Threading;
    using Logic;
    using Model;

    public class MainViewModel : ExtendedViewModelBase
    {
        private RelayCommand loadPostsCommand;
        private ObservableCollection<Post> posts = new ObservableCollection<Post>();
        private ObservableCollection<Post> newPosts = new ObservableCollection<Post>();
        private bool isLoading;
        private bool isStart = true;
        private ObservableCollection<Post> listToUse;
        private int inboxCount;

        public string SearchTag { get; set; }

        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            set
            {
                isLoading = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsStart
        {
            get
            {
                return isStart;
            }
            set
            {
                isStart = value;
                this.OnPropertyChanged();
            }
        }

        public RelayCommand LoadPostsCommand
        {
            get
            {
                return loadPostsCommand ?? (loadPostsCommand = new RelayCommand(Pr0grammService.LoadTopPosts));
            }
        }

        public ObservableCollection<Post> Posts
        {
            get
            {
                return posts;
            }
            set
            {
                posts = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Post> NewPosts
        {
            get
            {
                return newPosts;
            }
            set
            {
                newPosts = value;
                this.OnPropertyChanged();
            }
        }

        public ObservableCollection<Post> ListToUse
        {
            get
            {
                return listToUse;
            }
            set
            {
                listToUse = value;
                this.OnPropertyChanged();
            }
        }

        public int InboxCount
        {
            get
            {
                return inboxCount;
            }
            set
            {
                inboxCount = value;
                this.OnPropertyChanged();
            }
        }

        public Action ActionForReloadingPost { get; set; }

        public UserProfile UserProfile { get; set; }

        public Category Category { get; set; }

        public bool UserProfileCollection { get; set; }

        public bool UserProfileNavigationHistory { get; set; }

        public Post LastPostOpened { get; set; }
    }

    public enum Category
    {
        New = 0,
        Hot = 1
    }
}