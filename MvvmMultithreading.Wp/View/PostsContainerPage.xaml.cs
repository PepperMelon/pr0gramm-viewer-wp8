using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Pr0gramm.View
{
    using System.ComponentModel;
    using System.Windows.Threading;
    using Constants;
    using Extensions;
    using Logic;
    using Microsoft.Phone.Tasks;
    using Model;
    using Resources;
    using ViewModel;

    public partial class PostsContainerPage : PhoneApplicationPage
    {
        private int previousIndex;
        private CustomMessageBox customMessageBox;
        private bool customMessageBoxVisible;
        public DispatcherTimer slideshowTimer;
        public bool slideShowActive;
        public bool showPictureOnFullyLoaded;
        private bool imageZoomWasActive;

        public PostsContainerPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            this.BackKeyPress += OnBackKeyPress;
            var downloadButton = new ApplicationBarIconButton();
            downloadButton.Text = AppResources.Save;
            downloadButton.IconUri = new Uri(@"Resources\Download.png", UriKind.Relative);
            downloadButton.Click += DownloadFileTap;
            ApplicationBar.Buttons.Add(downloadButton);

            if (ViewModelLocator.Authentication.IsAuthenticated)
            {
                var commentButton = new ApplicationBarIconButton();
                commentButton.Text = AppResources.Comment;
                commentButton.IconUri = new Uri(@"Resources\Message.png", UriKind.Relative);
                commentButton.Click += ShowCommentFieldTap;
                ApplicationBar.Buttons.Add(commentButton);

                var addTagButton = new ApplicationBarIconButton();
                addTagButton.Text = AppResources.AddTag;
                addTagButton.IconUri = new Uri(@"Resources\Tag.png", UriKind.Relative);
                addTagButton.Click += ShowAddTagTextField;
                ApplicationBar.Buttons.Add(addTagButton);
            }

            if (Helper.StringToBoolean(ViewModelLocator.Settings.ShowSlideshowButton))
            {
                slideshowTimer = new DispatcherTimer();
                slideshowTimer.Tick += LoadNextPostTick;

                var seconds = 10;
                if (!string.IsNullOrEmpty(ViewModelLocator.Settings.SlideShowSecondsToChange))
                {
                    seconds = Convert.ToInt32(ViewModelLocator.Settings.SlideShowSecondsToChange);
                }

                slideshowTimer.Interval = TimeSpan.FromSeconds(seconds);

                var playPauseButton = new ApplicationBarIconButton();
                playPauseButton.Text = AppResources.Play;
                playPauseButton.IconUri = new Uri(@"Resources\play.png", UriKind.Relative);
                playPauseButton.Click += PlaySlideShow;
                ApplicationBar.Buttons.Add(playPauseButton);
            }
        }

        public void LoadNextPost()
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage);
            var targetIndex = GetNextTargetIndex(currentPost.CurrentPostViewModel);
            DeInitAllTheObjects();

            GetNextPost(currentPost.CurrentPostViewModel, targetIndex);
            currentPost.InitializePost(); 
        }

        public void LoadPreviousPost()
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage);
            var targetIndex = GetPreviousTargetIndex(currentPost.CurrentPostViewModel);
            DeInitAllTheObjects();

            GetPreviousPost(currentPost.CurrentPostViewModel, targetIndex);
            currentPost.InitializePost();
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;

            var shareMedia = new ApplicationBarMenuItem(AppResources.SharePicture);
            shareMedia.Click += ShareMediaButtonPressed;

            var shareLink = new ApplicationBarMenuItem(AppResources.Share);
            shareLink.Click += ShareButtonPressed;

            var shareProgrammLink = new ApplicationBarMenuItem(AppResources.SharePr0grammLink);
            shareProgrammLink.Click += SharePr0grammLinkPressed;

            var copyLink = new ApplicationBarMenuItem(AppResources.CopyLink);
            copyLink.Click += CopyLinkOnClick;

            ApplicationBar.MenuItems.Add(shareMedia);
            ApplicationBar.MenuItems.Add(shareLink);
            ApplicationBar.MenuItems.Add(shareProgrammLink);
            ApplicationBar.MenuItems.Add(copyLink);
        }
        
        private void LoadNextPostTick(object sender, EventArgs e)
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage);
            var targetIndex = GetNextTargetIndex(currentPost.CurrentPostViewModel);
            DeInitAllTheObjects();

            GetNextPost(currentPost.CurrentPostViewModel, targetIndex);
            currentPost.InitializePost();

            if (currentPost.CurrentPostViewModel.Post.Image.EndsWith(".gif") || currentPost.CurrentPostViewModel.Post.Image.EndsWith(".webm"))
            {
                currentPost.DisableImageZoom();
            }
            else
            {
                if (currentPost.wasImageZoomActive)
                {
                    slideshowTimer.Stop();
                    showPictureOnFullyLoaded = true;
                    currentPost.ZoomImage();
                }
            }
        }

        private void PlaySlideShow(object sender, EventArgs e)
        {
            ApplicationBarIconButton btn;
            if (ViewModelLocator.Authentication.IsAuthenticated)
            {
                btn = (ApplicationBarIconButton)ApplicationBar.Buttons[3];
            }
            else
            {
                btn = (ApplicationBarIconButton)ApplicationBar.Buttons[1];
            }

            if (btn.Text.Equals(AppResources.Play))
            {
                slideshowTimer.Start();
                btn.Text = AppResources.Pause;
                btn.IconUri = new Uri(@"Resources\pause.png", UriKind.Relative);
                slideShowActive = true;
            }
            else
            {
                slideshowTimer.Stop();
                btn.Text = AppResources.Play;
                btn.IconUri = new Uri(@"Resources\play.png", UriKind.Relative);
                slideShowActive = false;
            }
        }

        private void ShowAddTagTextField(object sender, EventArgs e)
        {
            var currentPostViewModel = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage).CurrentPostViewModel;
            var stackPanel = new StackPanel();
            var tagsText = new TextBox();
            stackPanel.Children.Add(tagsText);

            customMessageBox = new CustomMessageBox()
            {
                Caption = AppResources.Tags,
                Content = stackPanel,
                LeftButtonContent = AppResources.Add,
                RightButtonContent = AppResources.Cancel
            };

            customMessageBoxVisible = true;

            customMessageBox.Dismissed += async (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        await Pr0grammService.AddTags(currentPostViewModel, tagsText.Text);
                        customMessageBoxVisible = false;
                        break;
                    case CustomMessageBoxResult.RightButton:
                        customMessageBoxVisible = false;
                        break;
                    case CustomMessageBoxResult.None:
                        customMessageBoxVisible = false;
                        break;
                }
            };

            customMessageBox.Show();
            tagsText.UpdateLayout();
            tagsText.Focus();
        }

        private void OnBackKeyPress(object sender, CancelEventArgs cancelEventArgs)
        {
            DeInitContainer(sender, cancelEventArgs);
        }

        public void DeInitContainer(object sender, CancelEventArgs cancelEventArgs)
        {
            if (customMessageBox != null && customMessageBoxVisible)
            {
                customMessageBox.Dismiss();
                return;
            }

            var pivotItem = this.Pivot.SelectedItem as PivotItem;
            var postPage = pivotItem.Content as PostPage;
            postPage.OnBackKeyPress(sender, cancelEventArgs);
            if (!postPage.Exit)
            {
                cancelEventArgs.Cancel = true;
                return;
            }

            if (this.slideshowTimer != null)
            {
                this.slideshowTimer.Stop();
                this.slideshowTimer.Tick -= LoadNextPostTick;
                this.slideshowTimer = null;
            }

            this.PrevPost.Dispose();
            this.NextPost.Dispose();
            this.CurrentPost.Dispose();

            this.PrevPost = null;
            this.NextPost = null;
            this.CurrentPost = null;
        }

        private void Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            var targetIndex = 0;

            if ((pivot.SelectedIndex > previousIndex || (previousIndex == 2 && pivot.SelectedIndex == 0)) && !(previousIndex == 0 && pivot.SelectedIndex == 2))
            {
                if (pivot.SelectedIndex == 0)
                {
                    targetIndex = GetNextTargetIndex(this.NextPost.CurrentPostViewModel);
                    DeInitAllTheObjects();

                    GetNextPost(this.PrevPost.CurrentPostViewModel, targetIndex);
                    this.PrevPost.InitializePost();
                }

                if (pivot.SelectedIndex == 1)
                {
                    targetIndex = GetNextTargetIndex(this.PrevPost.CurrentPostViewModel);
                    DeInitAllTheObjects();
                    GetNextPost(this.CurrentPost.CurrentPostViewModel, targetIndex);
                    this.CurrentPost.InitializePost();
                }

                if (pivot.SelectedIndex == 2)
                {
                    targetIndex = GetNextTargetIndex(this.CurrentPost.CurrentPostViewModel);
                    DeInitAllTheObjects();
                    GetNextPost(this.NextPost.CurrentPostViewModel, targetIndex);
                    this.NextPost.InitializePost();
                }
            }
            else if (pivot.SelectedIndex < previousIndex || (previousIndex == 0 && pivot.SelectedIndex == 2))
            {
                if (pivot.SelectedIndex == 0)
                {
                    targetIndex = GetPreviousTargetIndex(this.CurrentPost.CurrentPostViewModel);
                    if (targetIndex < 0)
                    {
                        ViewModelLocator.Navigation.GoBack();
                        return;
                    }

                    DeInitAllTheObjects();
                    GetPreviousPost(this.PrevPost.CurrentPostViewModel, targetIndex);
                    this.PrevPost.InitializePost();
                }

                if (pivot.SelectedIndex == 1)
                {
                    targetIndex = GetPreviousTargetIndex(this.NextPost.CurrentPostViewModel);
                    if (targetIndex < 0)
                    {
                        ViewModelLocator.Navigation.GoBack();
                        return;
                    }

                    DeInitAllTheObjects();
                    GetPreviousPost(this.CurrentPost.CurrentPostViewModel, targetIndex);
                    this.CurrentPost.InitializePost();
                }

                if (pivot.SelectedIndex == 2)
                {
                    targetIndex = GetPreviousTargetIndex(this.PrevPost.CurrentPostViewModel);
                    if (targetIndex < 0)
                    {
                        ViewModelLocator.Navigation.GoBack();
                        return;
                    }

                    DeInitAllTheObjects();
                    GetPreviousPost(this.NextPost.CurrentPostViewModel, targetIndex);
                    this.NextPost.InitializePost();
                }
            }

            previousIndex = pivot.SelectedIndex;
        }

        private void DeInitAllTheObjects()
        {
            this.PrevPost.Dispose();
            this.CurrentPost.Dispose();
            this.NextPost.Dispose();
        }

        private int GetNextTargetIndex(CurrentPostViewModel dependentPostViewModel)
        {
            var targetIndex = 0;
            targetIndex = ViewModelLocator.Main.ListToUse.IndexOf(dependentPostViewModel.Post) + 1;

            return targetIndex;
        }

        private int GetPreviousTargetIndex(CurrentPostViewModel dependentPostViewModel)
        {
            var targetIndex = 0;
            targetIndex = ViewModelLocator.Main.ListToUse.IndexOf(dependentPostViewModel.Post) - 1;

            return targetIndex;
        }

        private bool GetPreviousPost(CurrentPostViewModel post, int targetIndex)
        {
            var prevPost = new Post();
            var indexOfprevPost = targetIndex;
            if (indexOfprevPost < 0)
            {
                return false;
            }

            prevPost = ViewModelLocator.Main.ListToUse.ElementAt(indexOfprevPost);

            post.Post = prevPost;
            return true;
        }

        private bool GetNextPost(CurrentPostViewModel post, int targetIndex)
        {
            var nexPost = new Post();
            var indexOfNextPost = targetIndex;

            if ((ViewModelLocator.Main.ListToUse.Count - indexOfNextPost) <= 5)
            {
                ViewModelLocator.Main.ActionForReloadingPost();
            }

            if (indexOfNextPost >= ViewModelLocator.Main.ListToUse.Count)
            {
                ViewModelLocator.Navigation.GoBack();
                return false;
            }

            nexPost = ViewModelLocator.Main.ListToUse.ElementAt(indexOfNextPost);

            if (post.Post != null && post.Post.CancellationTokenSource != null)
            {
                post.Post.CancellationTokenSource.Cancel(true);
            }

            var points = nexPost.Up - nexPost.Down;
            var targetPoints = 0;
            if (!string.IsNullOrEmpty(ViewModelLocator.Settings.PostPoints))
            {
                targetPoints = Convert.ToInt32(ViewModelLocator.Settings.PostPoints);
            }

            if (Helper.StringToBoolean(ViewModelLocator.Settings.OnlyPositivePosts) && points <= targetPoints)
            {
                ViewModelLocator.Main.ListToUse.Remove(nexPost);

                GetNextPost(post, targetIndex);
                return true;
            }

            post.Post = nexPost;
            return true;
        }

        private void ShareMediaButtonPressed(object sender, EventArgs e)
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage).CurrentPostViewModel.Post;
            Helper.OnImageSaved += HelperOnOnImageSaved;
            Helper.DownloadImage(currentPost.Image, currentPost.Id.ToString());
        }

        private void HelperOnOnImageSaved(object sender, string imagePath)
        {
            Helper.OnImageSaved -= HelperOnOnImageSaved;
            var shareMediaTask = new ShareMediaTask();
            shareMediaTask.FilePath = imagePath;
            shareMediaTask.Show();
        }

        private void ShareButtonPressed(object sender, EventArgs e)
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage).CurrentPostViewModel.Post;
            var shareLinkTask = new ShareLinkTask();
            shareLinkTask.LinkUri = new Uri(currentPost.Image, UriKind.Absolute);
            if (currentPost.Image.EndsWith(".gif"))
            {
                shareLinkTask.Title = AppResources.GifLink;
            }
            else if (currentPost.Image.EndsWith(".webm"))
            {
                shareLinkTask.Title = AppResources.WebmLink;
            }
            else
            {
                shareLinkTask.Title = AppResources.PictureLink;
            }

            shareLinkTask.Show();
        }

        private void SharePr0grammLinkPressed(object sender, EventArgs e)
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage).CurrentPostViewModel.Post;
            var shareLinkTask = new ShareLinkTask();
            var pr0grammLink = string.Empty;

            pr0grammLink = string.Format(ApiCalls.NewShareUrl, currentPost.Id);
            shareLinkTask.Title = AppResources.NewPostLink;

            shareLinkTask.LinkUri = new Uri(pr0grammLink, UriKind.Absolute);

            shareLinkTask.Show();
        }

        private void DownloadFileTap(object sender, EventArgs e)
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage).CurrentPostViewModel.Post;
            Helper.DownloadImage(currentPost.Image, currentPost.Id.ToString());
        }

        private void ShowCommentFieldTap(object sender, EventArgs e)
        {
            var postPage = (this.Pivot.SelectedItem as PivotItem).Content as PostPage;
            postPage.CommentContainer.Visibility = Visibility.Visible;
            postPage.CommentTextbox.Focus();
        }

        private void CopyLinkOnClick(object sender, EventArgs eventArgs)
        {
            var currentPost = ((this.Pivot.SelectedItem as PivotItem).Content as PostPage).CurrentPostViewModel.Post;
            var pr0grammLink = string.Format(ApiCalls.NewShareUrl, currentPost.Id);

            Clipboard.SetText(pr0grammLink);
            ViewModelLocator.ShowNotification(AppResources.LinkCopied, string.Empty);
        }
    }
}