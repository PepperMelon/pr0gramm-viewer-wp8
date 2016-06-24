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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.Linq;
    using System.Diagnostics;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;
    using Coding4Fun.Toolkit.Controls;
    using Constants;
    using Extensions;
    using Logic;
    using Microsoft.Phone.Tasks;
    using Model;
    using Resources;
    using ViewModel;

    public partial class PostPage : PhoneApplicationPage, IDisposable
    {
        public static readonly DependencyProperty CurrentPostViewModelProperty =
            DependencyProperty.Register("CurrentPostViewModel", typeof(CurrentPostViewModel), typeof(PostPage), new PropertyMetadata(default(CurrentPostViewModel)));

        public CurrentPostViewModel CurrentPostViewModel
        {
            get
            {
                if (this.DataContext != null && this.DataContext != typeof(PostsContainerViewModel))
                {
                    return (CurrentPostViewModel)this.DataContext;
                }

                return (CurrentPostViewModel)GetValue(CurrentPostViewModelProperty);
            }
            set { SetValue(CurrentPostViewModelProperty, value); }
        }

        public bool Exit;

        private DispatcherTimer dispatcherTimer = new DispatcherTimer();
        public BitmapImage image = new BitmapImage();
        public BitmapImage tempImage;
        private bool isCommentNavBarControlActive;
        //private static bool linkClicked;
        public bool isImageZoomActive;
        public bool wasImageZoomActive;
        private Vote voted;
        private WebBrowser browser;
        private CustomMessageBox voteTagCustomControl;
        private bool customMessageBoxVisible;
        private bool bigBrowserVisible = false;

        public PostPage()
        {
            InitializeComponent();
            this.progressBar1.Visibility = Visibility.Collapsed;
            this.CommentContainer.Visibility = Visibility.Collapsed;

            var showNavigationButtons = Helper.StringToBoolean(ViewModelLocator.Settings.ShouldShowNavigationButtons) ? Visibility.Visible : Visibility.Collapsed;
            this.NextPostButton.Visibility = showNavigationButtons;
            this.PreviousPostButton.Visibility = showNavigationButtons;

            var shouldEnableManualCommentsLoading = Helper.StringToBoolean(ViewModelLocator.Settings.ManualCommentsLoading);
            if (!shouldEnableManualCommentsLoading)
            {
                LoadCommentsContainer.Visibility = Visibility.Collapsed;
            }

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.BigBrowser.Visibility = Visibility.Collapsed;
            this.viewport.Visibility = Visibility.Collapsed;
            this.viewport.IsEnabled = false;
            this.CommentContainer.Visibility = Visibility.Collapsed;
            this.progressBar.Visibility = Visibility.Collapsed;
            this.DataContext = CurrentPostViewModel;
            this.BackKeyPress += OnBackKeyPress;
            InitBrowser();

            if (image == null)
            {
                this.image = new BitmapImage();
            }

            this.image.DownloadProgress += LoadingImageProgress;
            Pr0grammService.CurrentPostReloaded += Pr0GrammServiceOnCurrentPostReloaded;
            dispatcherTimer.Tick += DispatcherTimerOnTick;

            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                this.DownVoteButton.Visibility = Visibility.Collapsed;
                this.UpVoteButton.Visibility = Visibility.Collapsed;
                this.FavoriteButton.Visibility = Visibility.Collapsed;
            }

            InitializePost();
        }

        private void InitBrowser()
        {
            this.browser = new WebBrowser();
            this.browser.Name = "brauser";
            this.browser.IsHitTestVisible = false;
            this.BrowserPanel.Children.Clear();
            this.BrowserPanel.Children.Add(browser);
            this.browser.IsScriptEnabled = true;
            this.browser.ScriptNotify += WebBrowser_ScriptNotify;
            this.browser.VerticalAlignment = VerticalAlignment.Top;
            this.browser.Navigated += BrowserOnNavigated;
            this.browser.Navigating += BrowserOnNavigating;
        }

        private void Pr0GrammServiceOnCurrentPostReloaded(object sender, EventArgs eventArgs)
        {
            //this.Points.Text = new UpDownVotePointCoverter().Convert(CurrentPostViewModel.Post, null, null, null).ToString();
        }

        public async void InitializePost()
        {
            try
            {
                this.errortxb.Visibility = Visibility.Collapsed;
                this.progressBar.Visibility = Visibility.Visible;
                var currentPost = CurrentPostViewModel.Post;
                if (currentPost == null)
                {
                    return;
                }

                await GetVoteState(currentPost);
                this.CommentContainer.Visibility = Visibility.Collapsed;
                this.Points.Text = new UpDownVotePointCoverter().Convert(CurrentPostViewModel.Post, null, null, null).ToString();

                var shouldEnableManualCommentsLoading = Helper.StringToBoolean(ViewModelLocator.Settings.ManualCommentsLoading);
                if (!shouldEnableManualCommentsLoading)
                {
                    Pr0grammService.LoadPost(CurrentPostViewModel);
                }
                else
                {
                    if (CurrentPostViewModel.PostInfo != null)
                    {
                        CurrentPostViewModel.PostInfo.Comments.Clear();
                        CurrentPostViewModel.PostInfo.Tags.Clear();
                    }

                    LoadCommentsContainer.Visibility = Visibility.Visible;
                }

                //Pr0grammService.ReloadCurrentPost(CurrentPostViewModel);
                if (currentPost.Image.EndsWith(".gif"))
                {
                    EnableSharePicture(false);
                    InitBrowser();
                    dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
                    dispatcherTimer.Start();
                    this.img.Visibility = Visibility.Collapsed;
                    this.browser.Visibility = Visibility.Visible;
                    this.webmtxtb.Visibility = Visibility.Collapsed;
                    this.progressBar1.Visibility = Visibility.Collapsed;
                    if (this.browser != null)
                    {
                        ViewImageInWebBrowser(currentPost.Image);
                    }
                }
                else if (currentPost.Image.EndsWith(".webm"))
                {
                    EnableSharePicture(false);
                    this.webmtxtb.Visibility = Visibility.Visible;
                    this.webmtxtb.Text = "Webm";
                    InitBrowser();
                    dispatcherTimer.Interval = TimeSpan.FromMilliseconds(500);
                    dispatcherTimer.Start();
                    this.img.Visibility = Visibility.Collapsed;
                    this.browser.Visibility = Visibility.Visible;
                    this.progressBar1.Visibility = Visibility.Collapsed;
                    if (this.browser != null)
                    {
                        LoadWebm(currentPost.Image);
                    }
                }
                else
                {
                    if (image == null)
                    {
                        image = new BitmapImage();
                        image.DownloadProgress += LoadingImageProgress;
                    }

                    var container = GetPostContainer();
                    if (container.slideShowActive && isImageZoomActive)
                    {
                        if (this.progressBar1.Visibility != Visibility.Visible)
                        {
                            this.progressBar1.Visibility = Visibility.Visible;
                        }

                        tempImage = new BitmapImage();
                        tempImage.DownloadProgress += LoadingImageProgress;
                        tempImage.UriSource = new Uri(currentPost.Image);
                        this.img.Source = tempImage;
                    }
                    else
                    {
                        image.CreateOptions = BitmapCreateOptions.BackgroundCreation;
                        image.UriSource = new Uri(currentPost.Image);
                        this.img.Source = image;
                        this.progressBar1.Visibility = Visibility.Collapsed;
                    }

                    EnableSharePicture(true);
                    this.img.Visibility = Visibility.Visible;
                    this.webmtxtb.Visibility = Visibility.Collapsed;
                    dispatcherTimer.Stop();
                }

                this.progressBar.Visibility = Visibility.Collapsed;
                this.mainScrollViewer.ScrollToVerticalOffset(0);
            }
            catch (Exception)
            {
                OnBackKeyPress(null);
            }
        }

        private void EnableSharePicture(bool enable)
        {
            var appBar = GetPostContainer().ApplicationBar;
            (appBar.MenuItems[0] as ApplicationBarMenuItem).IsEnabled = enable;
        }

        private void LoadingImageProgress(object sender, DownloadProgressEventArgs e)
        {
            if (this.isImageZoomActive)
            {
                var container = GetPostContainer();
                if (container.slideShowActive)
                {
                    CurrentPostViewModel.ProgressValue = e.Progress;
                }
            }

            if (e.Progress == 100)
            {
                this.progressBar.Visibility = Visibility.Collapsed;
                if (this.isImageZoomActive)
                {
                    var pivotItem = this.Parent as PivotItem;
                    var container = ((pivotItem.Parent as Pivot).Parent as Grid).Parent as PostsContainerPage;
                    if (container.slideShowActive)
                    {
                        this.image = this.tempImage;
                        container.showPictureOnFullyLoaded = false;
                        CurrentPostViewModel.ProgressValue = 0;
                        ZoomImage();
                        container.slideshowTimer.Start();
                    }
                    else
                    {
                        this.TestImage.Source = this.image;
                    }
                }

                return;
            }

            if (this.progressBar.Visibility != Visibility.Visible)
            {
                this.progressBar.Visibility = Visibility.Visible;
                return;
            }
        }

        private async Task GetVoteState(Post currentPost)
        {
            if (currentPost == null)
            {
                return;
            }

            if (ViewModelLocator.VoteObject == null)
            {
                return;
            }

            if (!ViewModelLocator.Authentication.IsAuthenticated)
            {
                return;
            }

            this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));

            var votingPost =
                ViewModelLocator.VoteItemViewModel.Items.Where(x => x.VoteType == VoteType.Post)
                    .FirstOrDefault(x => x.Id == currentPost.Id);

            if (votingPost == null)
            {
                this.UpVoteButton.IsChecked = false;
                this.DownVoteButton.IsChecked = false;
                voted = Vote.Neutral;
                return;
            }

            if (votingPost.Vote == Vote.Neutral)
            {
                this.UpVoteButton.IsChecked = false;
                this.DownVoteButton.IsChecked = false;
            }
            else if (votingPost.Vote == Vote.Up)
            {
                this.UpVoteButton.IsChecked = true;
                this.DownVoteButton.IsChecked = false;
            }
            else if (votingPost.Vote == Vote.Down)
            {
                this.UpVoteButton.IsChecked = false;
                this.DownVoteButton.IsChecked = true;
            }
            else if (votingPost.Vote == Vote.Favorite)
            {
                this.UpVoteButton.IsChecked = true;
                this.DownVoteButton.IsChecked = false;
                this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
            }

            this.voted = votingPost.Vote;
        }

        private int counter = 0;
        private void DispatcherTimerOnTick(object sender, EventArgs eventArgs)
        {
            try
            {
                if (counter > 1)
                {
                    this.dispatcherTimer.Stop();
                    counter = 0;
                }

                if (browser != null && browser.Visibility == Visibility.Visible)
                {
                    counter++;
                    this.browser.InvokeScript("getSrcSize");
                }
            }
            catch
            {
                return;
            }
        }

        private void BrowserOnNavigated(object sender, NavigationEventArgs navigationEventArgs)
        {
            try
            {
                browser.InvokeScript("getSrcSize");
            }
            catch
            {
                this.browser.Opacity = 1.0;
                return;
            }

            this.browser.Opacity = 1.0;
        }

        private void BrowserOnNavigating(object sender, NavigatingEventArgs navigatingEventArgs)
        {
            this.browser.Opacity = 0;
        }

        public void OnBackKeyPress(object sender, CancelEventArgs cancelEventArgs)
        {
            DeInitPostPage(cancelEventArgs);
        }

        private void DeInitPostPage(CancelEventArgs cancelEventArgs)
        {
            if (voteTagCustomControl != null && customMessageBoxVisible)
            {
                voteTagCustomControl.Dismiss();
                return;
            }

            if (bigBrowserVisible)
            {
                this.BigBrowser.Visibility = Visibility.Collapsed;
                //this.BigBrowser = null;
                this.BigBrowser.ClearCookiesAsync();
                this.BigBrowser.ClearInternetCacheAsync();
                this.BigBrowser.NavigateToString(string.Empty);
                this.bigBrowserVisible = false;
                cancelEventArgs.Cancel = true;
                return;
            }

            if (isImageZoomActive)
            {
                this.progressBar1.Visibility = Visibility.Collapsed;
                cancelEventArgs.Cancel = true;
                this.wasImageZoomActive = false;
                DisableImageZoom();
                return;
            }

            if (isCommentNavBarControlActive)
            {
                cancelEventArgs.Cancel = true;

                if (this.CommentNavControl.CommentContainer.Visibility == Visibility.Visible)
                {
                    this.CommentNavControl.CommentTextbox.Text = string.Empty;
                    this.CommentNavControl.MenuContainer.Visibility = Visibility.Visible;
                    this.CommentNavControl.CommentContainer.Visibility = Visibility.Collapsed;
                    return;
                }

                CommentNavControlSlideOutFromBottom.Begin();
                isCommentNavBarControlActive = false;
                UnlockPivot();
                mainScrollViewer.IsHitTestVisible = true;

                return;
            }

            this.BackKeyPress -= OnBackKeyPress;
            DeinitBrowser();

            if (image != null)
            {
                image.DownloadProgress -= LoadingImageProgress;
            }

            if (dispatcherTimer != null)
            {
                dispatcherTimer.Tick -= DispatcherTimerOnTick;
                this.dispatcherTimer.Stop();
            }

            this.browser = null;
            Dispose();
            Exit = true;
        }

        public void DisableImageZoom()
        {
            UnlockPivot();
            this.isImageZoomActive = false;
            this.TestImage.Source = null;
            var appBar = ((((this.Parent as PivotItem).Parent as Pivot).Parent as Grid).Parent as PostsContainerPage).ApplicationBar;
            appBar.IsVisible = true;
            this.ContentPanel.Visibility = Visibility.Visible;
            this.viewport.Visibility = Visibility.Collapsed;
            this.viewport.IsEnabled = false;
            SystemTray.IsVisible = true;
            this.titleStackPanel.Visibility = Visibility.Visible;
            this.img.Source = image;
        }

        private void UnlockPivot()
        {
            var pivotItem = this.Parent as PivotItem;
            var pivot = pivotItem.Parent as Pivot;
            pivot.IsLocked = false;
        }

        private void DeinitBrowser()
        {
            var webBrowser = this.browser;
            if (webBrowser != null)
            {
                this.BrowserPanel.Children.Remove(webBrowser);
                webBrowser.Navigated -= BrowserOnNavigated;
                webBrowser.Navigating -= BrowserOnNavigating;
            }
        }

        private void LoadWebm(string postUrl)
        {
            var urloMpg = postUrl.Replace(".webm", ".mpg");
            browser.IsEnabled = true;
            browser.IsScriptEnabled = true;
            browser.Visibility = Visibility.Visible;
            browser.Width = 500;
            browser.Height = 480;

            browser.Navigated += (sender, args) =>
            {
                try
                {
                    PlayWebm(urloMpg);
                }
                catch (Exception)
                {
                    //ViewModelLocator.ShowNotification("Fehler beim Dekodieren der Webm", string.Empty);
                }
            };

            var pr0grammLink = "http://pr0gramm.com/new/webm/512693";
            browser.Navigate(new Uri(pr0grammLink));
        }

        private int webmReloadCounter;
        private void PlayWebm(string urloMpg)
        {
            try
            {
                var height = 500;
                var width = 480;
                if (App.Current.Host.Content.ScaleFactor == 100)
                {
                    height = 303;
                    width = 305;
                }
                else if (App.Current.Host.Content.ScaleFactor == 160)
                {
                    height = 377;
                    width = 379;
                }
                else if (App.Current.Host.Content.ScaleFactor == 150)
                {
                    height = 303;
                    width = 305;
                }

                //377 device
                //305/303 emu
                string script = "$('div').remove(); " +
                                "var canvas = document.createElement('canvas');" +
                                "canvas.className = 'item-image';" +
                                "document.body.appendChild( canvas ); " +
                                "$('.item-image').appendTo('body'); " +
                                "$('.item-image').width(browserWidth); " +
                                "$('.item-image').height(browserHeight); " +
                                "var canvas = document.getElementsByClassName('item-image')[0]; " +
                                "var clientHeight = canvas.clientHeight;" +
                                "var script = document.createElement('script');" +
                                "script.src = 'http://pr0gramm.com/frontend/lib/jsmpeg.min.js'; " +
                                "script.onload = function(){ this.jsmpeg = new jsmpeg('replaceContent', {canvas:canvas, autoplay:true, loop: true}); }; " +
                                "document.body.appendChild( script ); " +
                                "$('html').scrollTop(0); " +
                                "$( document ).ready(function() { $('html, body').animate({ scrollTop: '0px' }, 'fast'); });";

                script = script.Replace("replaceContent", urloMpg).Replace("browserWidth", width.ToString()).Replace("browserHeight", height.ToString());
                browser.InvokeScript("eval", script);
            }
            catch (Exception)
            {
                webmReloadCounter++;
                if (webmReloadCounter > 10)
                {
                    DeinitBrowser();
                    this.errortxb.Visibility = Visibility.Visible;
                    this.errortxb.Text = AppResources.CouldNotLoadWebm;
                    return;
                }

                PlayWebm(urloMpg);
            }
        }

        private void ViewImageInWebBrowser(string ImageURL)
        {
            browser.IsEnabled = true;
            browser.Visibility = Visibility.Visible;

            var colorToUse = "#000000";
            if (ViewModelLocator.Settings.Theme == "Light Theme")
            {
                colorToUse = "#FFFFFF";
            }

            const string htmlScript = @"<script type='text/javascript' language'javascript'> 
                                            window.onload = function () { 
                                                getSrcSize();
                                            } 
                                            
                                            function getSrcSize() {
                                                var elem = document.getElementById('content'); 
                                                window.external.notify(elem.offsetHeight  + '');  
return;
                                                if (elem.width > elem.height)
                                                {
                                                    window.external.notify(elem.height  + '');  
                                                }
                                                else
                                                {
                                                    window.external.notify(document.getElementsByTagName('body')[0].height  + '');  
                                                }
                                            }

                                            function fitScreen() {
                                                document.getElementById('pageWrapper').style['width'] = '100%';
                                                window.scrollTo(0,0);
                                            }
                                        </script>
                                         <style>
                                                    html, body {
                                                        margin: 0;
                                                        padding: 0;
                                              }
                                            </style>";

            string imageHTML = "<div id=\"content\"><IMG id=\"pageWrapper\" style=\"width:100%\" SRC=\"" + ImageURL + "\"></div>";

            var htmlConcat = string.Format("<html>" +
                                           "<head>" +
                                           "<meta name=\"viewport\" content=\"width=device-width\" /> {0}" +
                                           "</head>" +
                                           "<body bgcolor=\"Replacecolor\" style=\"position:fixed\">" +
                                           "{1}</body></html>",
                                           htmlScript,
                                           imageHTML);

            htmlConcat = htmlConcat.Replace("Replacecolor", colorToUse);

            browser.NavigateToString(htmlConcat);
        }

        public void CallFitScreen()
        {
            browser.InvokeScript("fitScreen");
        }

        private void WebBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            try
            {
                this.browser.Height = Convert.ToDouble(e.Value) + 120;
                CallFitScreen();
            }
            catch
            {
                return;
            }
        }

        private void OnPostTap(object sender, GestureEventArgs e)
        {
            if (isCommentNavBarControlActive)
            {
                return;
            }

            //if (linkClicked)
            //{
            //    linkClicked = false;
            //    return;
            //}

            var comment = ((Grid)sender).DataContext as Comment;
            if (this.CommentNavControl.Comment != null && this.CommentNavControl.Comment.Id == comment.Id)
            {
                this.CommentNavControl.Comment = null;
            }

            this.CommentNavControl.Comment = comment;
            this.CommentNavControl.CurrentPostViewModel = CurrentPostViewModel;
            CommentNavControlSlideInFromBottom.Begin();
            isCommentNavBarControlActive = true;
            LockPivot();
            mainScrollViewer.IsHitTestVisible = false;
        }

        private async void UpVoteTap(object sender, GestureEventArgs e)
        {
            var successfull = false;
            var currentPost = CurrentPostViewModel.Post;

            switch (voted)
            {
                case Vote.Neutral:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Up);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Up);

                        currentPost.Up++;
                        voted = Vote.Up;
                    }
                    else
                    {
                        this.UpVoteButton.IsChecked = false;
                    }
                    break;
                case Vote.Up:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Neutral);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Neutral);

                        currentPost.Up--;
                        voted = Vote.Neutral;
                    }
                    else
                    {
                        this.UpVoteButton.IsChecked = true;
                    }
                    break;
                case Vote.Down:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Up);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Up);

                        currentPost.Down--;
                        currentPost.Up++;
                        voted = Vote.Up;
                        this.DownVoteButton.IsChecked = false;
                    }
                    else
                    {
                        this.DownVoteButton.IsChecked = true;
                    }
                    break;
                case Vote.Favorite:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Neutral);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Neutral);

                        currentPost.Up--;
                        voted = Vote.Neutral;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));
                    }
                    else
                    {
                        this.UpVoteButton.IsChecked = true;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
                    }
                    break;
            }

            this.Points.Text = new UpDownVotePointCoverter().Convert(currentPost, null, null, null).ToString();
        }

        private static void SetVoteForPost(Post currentPost, Vote vote)
        {
            var votingPost = ViewModelLocator.VoteItemViewModel.Items.Where(x => x.VoteType == VoteType.Post).FirstOrDefault(x => x.Id == currentPost.Id);
            if (votingPost != null)
            {
                votingPost.Vote = vote;
            }
            else
            {
                votingPost = new VoteObjectItem() { Id = currentPost.Id, Vote = vote, VoteType = VoteType.Post };
                ViewModelLocator.VoteItemViewModel.Items.Add(votingPost);
            }
        }

        private async void DownVoteTap(object sender, GestureEventArgs e)
        {
            var successfull = false;
            var currentPost = CurrentPostViewModel.Post;
            switch (voted)
            {
                case Vote.Neutral:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Down);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Down);

                        currentPost.Down++;
                        voted = Vote.Down;
                    }
                    else
                    {
                        this.DownVoteButton.IsChecked = false;
                    }
                    break;
                case Vote.Up:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Down);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Down);

                        currentPost.Down++;
                        currentPost.Up--;
                        voted = Vote.Down;
                        this.UpVoteButton.IsChecked = false;
                    }
                    else
                    {
                        this.UpVoteButton.IsChecked = true;
                    }
                    break;
                case Vote.Down:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Neutral);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Neutral);

                        currentPost.Down--;
                        voted = Vote.Neutral;
                    }
                    else
                    {
                        this.DownVoteButton.IsChecked = true;
                    }
                    break;
                case Vote.Favorite:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Down);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Down);

                        currentPost.Down++;
                        currentPost.Up--;
                        voted = Vote.Down;
                        this.UpVoteButton.IsChecked = false;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));
                    }
                    else
                    {
                        this.UpVoteButton.IsChecked = true;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
                    }
                    break;
            }

            this.Points.Text = new UpDownVotePointCoverter().Convert(currentPost, null, null, null).ToString();
        }

        private async void FavoriteButton_OnTap(object sender, GestureEventArgs e)
        {
            var successfull = false;
            var currentPost = CurrentPostViewModel.Post;
            switch (voted)
            {
                case Vote.Neutral:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Favorite);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Favorite);

                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
                        currentPost.Up++;
                        voted = Vote.Favorite;
                        this.UpVoteButton.IsChecked = true;
                    }
                    else
                    {
                        this.UpVoteButton.IsChecked = false;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));
                    }
                    break;
                case Vote.Up:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Favorite);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Favorite);

                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
                        voted = Vote.Favorite;
                    }
                    else
                    {
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));
                    }
                    break;
                case Vote.Down:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Favorite);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Favorite);

                        currentPost.Down--;
                        currentPost.Up++;
                        voted = Vote.Favorite;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
                        this.DownVoteButton.IsChecked = false;
                        this.UpVoteButton.IsChecked = true;
                    }
                    else
                    {
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));
                        this.DownVoteButton.IsChecked = true;
                    }
                    break;
                case Vote.Favorite:
                    successfull = await Pr0grammService.VotePost(currentPost.Id.ToString(), Vote.Up);
                    if (successfull)
                    {
                        SetVoteForPost(currentPost, Vote.Up);

                        voted = Vote.Up;
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\GrayHeart.png", UriKind.Relative));
                        this.UpVoteButton.IsChecked = true;
                    }
                    else
                    {
                        this.FavoriteButton.Source = new BitmapImage(new Uri(@"..\Resources\OrangeHeart.png", UriKind.Relative));
                    }
                    break;
            }

            this.Points.Text = new UpDownVotePointCoverter().Convert(currentPost, null, null, null).ToString();
        }

        private async void SendTap(object sender, GestureEventArgs e)
        {
            var successfully = await Pr0grammService.Comment(this.CommentTextbox.Text, CurrentPostViewModel.Post.Id.ToString(), "0");
            if (successfully)
            {
                var mark = Convert.ToInt32(ViewModelLocator.Authentication.Mark);
                var comment = new Comment()
                {

                    Content = this.CommentTextbox.Text,
                    Down = 0,
                    Up = 1,
                    Mark = mark,
                    ReadableCreatedTime = DateTime.Now,
                    Name = ViewModelLocator.Authentication.UserName
                };

                CurrentPostViewModel.PostInfo.Comments.Add(comment);
            }
            else
            {
                ViewModelLocator.ShowNotification(AppResources.ErrorOnSending, string.Empty);
            }

            this.CommentTextbox.Text = string.Empty;
            this.CommentContainer.Visibility = Visibility.Collapsed;
        }

        private void CancelSendTap(object sender, GestureEventArgs e)
        {
            this.CommentTextbox.Text = string.Empty;
            this.CommentContainer.Visibility = Visibility.Collapsed;
        }

        internal void ZoomImage()
        {
            LockPivot();
            this.isImageZoomActive = true;
            this.wasImageZoomActive = true;
            this.ContentPanel.Visibility = Visibility.Collapsed;
            this.viewport.Visibility = Visibility.Visible;
            this.viewport.IsEnabled = true;
            this.titleStackPanel.Visibility = Visibility.Collapsed;
            SystemTray.IsVisible = false;
            DefaultImageSettings();

            var pivotItem = this.Parent as PivotItem;
            var container = ((pivotItem.Parent as Pivot).Parent as Grid).Parent as PostsContainerPage;
            if (!container.showPictureOnFullyLoaded)
            {
                this.TestImage.Source = image;
            }
        }

        private void LockPivot()
        {
            var pivotItem = this.Parent as PivotItem;
            var pivot = pivotItem.Parent as Pivot;
            pivot.IsLocked = true;
        }

        private void DefaultImageSettings()
        {
            _scale = 0;
            CoerceScale(true);
            _scale = _coercedScale;

            ResizeImage(true);
        }

        const double MaxScale = 10;

        double _scale = 1.0;
        double _minScale;
        double _coercedScale;
        double _originalScale;

        Size _viewportSize;
        bool _pinching;
        Point _screenMidpoint;
        Point _relativeMidpoint;

        /// <summary>  
        /// Either the user has manipulated the image or the size of the viewport has changed. We only  
        /// care about the size.  
        /// </summary>  
        void viewport_ViewportChanged(object sender, System.Windows.Controls.Primitives.ViewportChangedEventArgs e)
        {
            Size newSize = new Size(viewport.Viewport.Width, viewport.Viewport.Height);
            if (newSize != _viewportSize)
            {
                _viewportSize = newSize;
                CoerceScale(true);
                ResizeImage(false);
            }
        }

        /// <summary>  
        /// Handler for the ManipulationStarted event. Set initial state in case  
        /// it becomes a pinch later.  
        /// </summary>  
        void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            _pinching = false;
            _originalScale = _scale;
        }

        /// <summary>  
        /// Handler for the ManipulationDelta event. It may or may not be a pinch. If it is not a   
        /// pinch, the ViewportControl will take care of it.  
        /// </summary>  
        /// <param name="sender"></param>  
        /// <param name="e"></param>  
        void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            if (e.PinchManipulation != null)
            {
                e.Handled = true;

                if (!_pinching)
                {
                    _pinching = true;
                    Point center = e.PinchManipulation.Original.Center;
                    _relativeMidpoint = new Point(center.X / TestImage.ActualWidth, center.Y / TestImage.ActualHeight);

                    var xform = TestImage.TransformToVisual(viewport);
                    _screenMidpoint = xform.Transform(center);
                }

                _scale = _originalScale * e.PinchManipulation.CumulativeScale;

                CoerceScale(false);
                ResizeImage(false);
            }
            else if (_pinching)
            {
                _pinching = false;
                _originalScale = _scale = _coercedScale;
            }
        }

        /// <summary>  
        /// The manipulation has completed (no touch points anymore) so reset state.  
        /// </summary>  
        void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            _pinching = false;
            _scale = _coercedScale;
        }


        /// <summary>  
        /// When a new image is opened, set its initial scale.  
        /// </summary>  
        void OnImageOpened(object sender, RoutedEventArgs e)
        {
            DefaultImageSettings();
        }

        /// <summary>  
        /// Adjust the size of the image according to the coerced scale factor. Optionally  
        /// center the image, otherwise, try to keep the original midpoint of the pinch  
        /// in the same spot on the screen regardless of the scale.  
        /// </summary>  
        /// <param name="center"></param>  
        void ResizeImage(bool center)
        {
            if (_coercedScale != 0 && image != null)
            {
                double newWidth = canvas.Width = Math.Round(image.PixelWidth * _coercedScale);
                double newHeight = canvas.Height = Math.Round(image.PixelHeight * _coercedScale);

                xform.ScaleX = xform.ScaleY = _coercedScale;

                viewport.Bounds = new Rect(0, 0, newWidth, newHeight);

                if (center)
                {
                    viewport.SetViewportOrigin(
                        new Point(
                            Math.Round((newWidth - viewport.ActualWidth) / 2),
                            Math.Round((newHeight - viewport.ActualHeight) / 2)
                            ));
                }
                else
                {
                    Point newImgMid = new Point(newWidth * _relativeMidpoint.X, newHeight * _relativeMidpoint.Y);
                    Point origin = new Point(newImgMid.X - _screenMidpoint.X, newImgMid.Y - _screenMidpoint.Y);
                    viewport.SetViewportOrigin(origin);
                }
            }
        }

        /// <summary>  
        /// Coerce the scale into being within the proper range. Optionally compute the constraints   
        /// on the scale so that it will always fill the entire screen and will never get too big   
        /// to be contained in a hardware surface.  
        /// </summary>  
        /// <param name="recompute">Will recompute the min max scale if true.</param>  
        void CoerceScale(bool recompute)
        {
            if (recompute && image != null && viewport != null)
            {
                // Calculate the minimum scale to fit the viewport  
                double minX = viewport.ActualWidth / image.PixelWidth;
                double minY = viewport.ActualHeight / image.PixelHeight;

                _minScale = Math.Min(minX, minY);
            }

            _coercedScale = Math.Min(MaxScale, Math.Max(_scale, _minScale));

        }

        public void Dispose()
        {
            try
            {
                Dispose(true);
            }
            catch
            {
                Debug.WriteLine("Error on dispose");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.mainScrollViewer.ScrollToVerticalOffset(0);

                if (CurrentPostViewModel.PostInfo != null)
                {
                    CurrentPostViewModel.PostInfo.Comments.Clear();
                    CurrentPostViewModel.PostInfo = null;
                }

                if (CurrentPostViewModel.Post != null && CurrentPostViewModel.Post.CancellationTokenSource != null)
                {
                    CurrentPostViewModel.Post.CancellationTokenSource.Cancel();
                }

                CurrentPostViewModel.Post = null;

                if (browser != null)
                {
                    browser.Visibility = Visibility.Collapsed;
                    DeinitBrowser();
                    browser = null;
                }



                if (img != null)
                {
                    img.Source = null;
                }

                if (dispatcherTimer != null)
                {
                    dispatcherTimer.Stop();
                }

                var container = GetPostContainer();
                if (!container.slideShowActive && !wasImageZoomActive && image != null)
                {
                    image.UriSource = null;
                    image.DownloadProgress -= LoadingImageProgress;
                    image = null;
                }

                if (!container.slideShowActive && !wasImageZoomActive && tempImage != null)
                {
                    tempImage.UriSource = null;
                    tempImage.DownloadProgress -= LoadingImageProgress;
                    tempImage = null;
                }
            }
        }

        private PostsContainerPage GetPostContainer()
        {
            var pivotItem = this.Parent as PivotItem;
            var container = ((pivotItem.Parent as Pivot).Parent as Grid).Parent as PostsContainerPage;
            return container;
        }

        private void TagSelectionChanged(object sender, GestureEventArgs e)
        {
            TagInfo item;
            try
            {
                item = (sender as ListBox).SelectedItems[0] as TagInfo;
            }
            catch (Exception)
            {
                return;
            }

            if (item == null)
            {
                return;
            }

            var stackPanel = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Vertical
            };

            var buttonStackPanel = new StackPanel()
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal
            };

            stackPanel.Children.Add(buttonStackPanel);

            var upVote = new Button()
            {
                Width = 225,
                Height = 70,
                Content = AppResources.Like
            };

            var downVote = new Button()
            {
                Width = 225,
                Height = 70,
                Content = AppResources.Unlike
            };

            if (ViewModelLocator.Authentication.IsAuthenticated)
            {
                buttonStackPanel.Children.Add(upVote);
                buttonStackPanel.Children.Add(downVote);

                var currentAccentColorHex = (Color)Application.Current.Resources["PhoneAccentColor"];
                var votingTag =
                       ViewModelLocator.VoteItemViewModel.Items.Where(x => x.VoteType == VoteType.Tag)
                           .FirstOrDefault(x => x.Id == item.Id);

                if (votingTag != null)
                {
                    if (votingTag.Vote == Vote.Up)
                    {
                        upVote.Background = new SolidColorBrush(currentAccentColorHex);
                    }
                    else if (votingTag.Vote == Vote.Down)
                    {
                        downVote.Background = new SolidColorBrush(currentAccentColorHex);
                    }
                }

                upVote.Tap += async (o, args) =>
                {
                    if (votingTag == null)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Up);
                        upVote.Background = new SolidColorBrush(currentAccentColorHex);
                        votingTag = new VoteObjectItem()
                        {
                            Id = item.Id,
                            Vote = Vote.Up,
                            VoteType = VoteType.Tag
                        };

                        ViewModelLocator.VoteItemViewModel.Items.Add(votingTag);
                    }
                    else if (votingTag.Vote == Vote.Up)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Neutral);
                        upVote.Background = new SolidColorBrush(Colors.Transparent);
                        votingTag.Vote = Vote.Neutral;
                    }
                    else if (votingTag.Vote == Vote.Neutral)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Up);
                        upVote.Background = new SolidColorBrush(currentAccentColorHex);
                        votingTag.Vote = Vote.Up;
                    }
                    else if (votingTag.Vote == Vote.Down)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Up);
                        upVote.Background = new SolidColorBrush(currentAccentColorHex);
                        downVote.Background = new SolidColorBrush(Colors.Transparent);
                        votingTag.Vote = Vote.Up;
                    }
                };

                downVote.Tap += async (o, args) =>
                {
                    if (votingTag == null)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Down);
                        downVote.Background = new SolidColorBrush(currentAccentColorHex);
                        votingTag = new VoteObjectItem()
                        {
                            Id = item.Id,
                            Vote = Vote.Down,
                            VoteType = VoteType.Tag
                        };

                        ViewModelLocator.VoteItemViewModel.Items.Add(votingTag);
                    }
                    else if (votingTag.Vote == Vote.Up)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Down);
                        downVote.Background = new SolidColorBrush(currentAccentColorHex);
                        upVote.Background = new SolidColorBrush(Colors.Transparent);
                        votingTag.Vote = Vote.Down;
                    }
                    else if (votingTag.Vote == Vote.Neutral)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Down);
                        downVote.Background = new SolidColorBrush(currentAccentColorHex);
                        votingTag.Vote = Vote.Down;
                    }
                    else if (votingTag.Vote == Vote.Down)
                    {
                        await Pr0grammService.VoteTag(item, Vote.Neutral);
                        downVote.Background = new SolidColorBrush(Colors.Transparent);
                        votingTag.Vote = Vote.Neutral;
                    }
                };
            }

            var searchForTag = new Button()
            {
                Width = 225,
                Height = 70,
                Content = AppResources.BrowseTag,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            stackPanel.Children.Add(searchForTag);

            searchForTag.Tap += async (o, args) =>
            {
                if (ViewModelLocator.Main.UserProfileCollection && ViewModelLocator.Main.UserProfile != null)
                {
                    ViewModelLocator.Main.UserProfile.PostLikes.Clear();
                    ViewModelLocator.Main.UserProfile.PostUploads.Clear();
                    ViewModelLocator.Main.UserProfile = null;
                    ViewModelLocator.Main.UserProfileNavigationHistory = false;
                }

                DeInitPostPage(new CancelEventArgs());
                var postContainer = GetPostContainer();
                postContainer.DeInitContainer(this, new CancelEventArgs());
                ViewModelLocator.Main.SearchTag = item.Tag;
                Pr0grammService.ClearPosts();
                ViewModelLocator.Main.LoadPostsCommand.Execute(null);
                ViewModelLocator.Navigation.NavigateTo(PageUrls.MainPageUri);
                ViewModelLocator.Navigation.ClearBackstack(false);
            };

            voteTagCustomControl = new CustomMessageBox()
            {
                Caption = AppResources.VoteTag,
                Content = stackPanel,
                IsLeftButtonEnabled = false,
                IsRightButtonEnabled = false
            };

            customMessageBoxVisible = true;

            voteTagCustomControl.Dismissed += async (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.None:
                        customMessageBoxVisible = false;
                        break;
                }
            };

            voteTagCustomControl.Show();
        }

        //private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    var richtTextBox = sender as RichTextBox;
        //    var comment = (richtTextBox.Parent as Grid).DataContext as Comment;

        //    SetLinkedText(richtTextBox, comment.Content);

        //}

        //public static void SetLinkedText(RichTextBox richTextBlock, string htmlFragment)
        //{
        //    var linkParser = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //    richTextBlock.Blocks.Clear();

        //    int nextOffset = 0;

        //    foreach (Match match in linkParser.Matches(htmlFragment))
        //    {
        //        if (match.Index >= nextOffset)
        //        {
        //            AppendText(richTextBlock, htmlFragment.Substring(nextOffset, match.Index - nextOffset));

        //            try
        //            {
        //                AppendLink(richTextBlock, match.Value, new Uri(match.Value));
        //                nextOffset = match.Index + match.Length;
        //            }
        //            catch (Exception)
        //            {
        //                try
        //                {
        //                    AppendLink(richTextBlock, match.Value, new Uri(match.Value.Replace("www.", "https://")));
        //                    nextOffset = match.Index + match.Length;
        //                }
        //                catch
        //                {
        //                }
        //            }
        //        }
        //    }

        //    AppendText(richTextBlock, htmlFragment.Substring(nextOffset));
        //}

        //public static void AppendText(RichTextBox richTextBlock, string text)
        //{
        //    Paragraph paragraph;

        //    if (richTextBlock.Blocks.Count == 0 ||
        //        (paragraph = richTextBlock.Blocks[richTextBlock.Blocks.Count - 1] as Paragraph) == null)
        //    {
        //        paragraph = new Paragraph();
        //        richTextBlock.Blocks.Add(paragraph);
        //    }

        //    paragraph.Inlines.Add(new Run { Text = text });
        //}

        //public static void AppendLink(RichTextBox richTextBlock, string text, Uri uri)
        //{
        //    Paragraph paragraph;

        //    if (richTextBlock.Blocks.Count == 0 ||
        //        (paragraph = richTextBlock.Blocks[richTextBlock.Blocks.Count - 1] as Paragraph) == null)
        //    {
        //        paragraph = new Paragraph();
        //        richTextBlock.Blocks.Add(paragraph);
        //    }

        //    var run = new Run { Text = text };
        //    var link = new Hyperlink { NavigateUri = uri, TargetName = "_blank" };
        //    link.Command = new DummyCommand();

        //    link.Inlines.Add(run);
        //    paragraph.Inlines.Add(link);
        //}

        //public class DummyCommand : ICommand
        //{
        //    public bool CanExecute(object parameter)
        //    {
        //        return true;
        //    }

        //    public void Execute(object parameter)
        //    {
        //        linkClicked = true;
        //    }

        //    public event EventHandler CanExecuteChanged;
        //}

        private void NextPostButtonTap(object sender, GestureEventArgs e)
        {
            var container = GetPostContainer();
            container.LoadNextPost();
        }

        private void PreviousPostButtonTap(object sender, GestureEventArgs e)
        {
            var container = GetPostContainer();
            container.LoadPreviousPost();
        }

        private void LoadCommentsButtonTap(object sender, GestureEventArgs e)
        {
            var stackPanel = sender as StackPanel;
            Pr0grammService.LoadPost(CurrentPostViewModel);
            stackPanel.Visibility = Visibility.Collapsed;
        }

        private async void VisitProfileTab(object sender, GestureEventArgs e)
        {
            if (ViewModelLocator.Main.UserProfileCollection && ViewModelLocator.Main.UserProfile != null)
            {
                ViewModelLocator.Main.UserProfile.PostLikes.Clear();
                ViewModelLocator.Main.UserProfile.PostUploads.Clear();
                ViewModelLocator.Main.UserProfile = null;
            }


            var comment = (sender as MenuItem).DataContext as Comment;

            ViewModelLocator.Main.LastPostOpened = CurrentPostViewModel.Post;
            var user = await Pr0grammService.LoadUserProfileAsync(comment.Name);
            ViewModelLocator.Main.UserProfile = user;

            DeInitPostPage(new CancelEventArgs());
            var postContainer = GetPostContainer();
            postContainer.DeInitContainer(this, new CancelEventArgs());

            ViewModelLocator.Navigation.NavigateTo(PageUrls.UserProfilePageUrl);

            if (ViewModelLocator.Main.UserProfileNavigationHistory)
            {
                ViewModelLocator.Navigation.ClearBackstack(true);
            }
        }

        private async void VisitOPProfile(object sender, GestureEventArgs e)
        {
            if (ViewModelLocator.Main.UserProfileCollection && ViewModelLocator.Main.UserProfile != null)
            {
                ViewModelLocator.Main.UserProfile.PostLikes.Clear();
                ViewModelLocator.Main.UserProfile.PostUploads.Clear();
                ViewModelLocator.Main.UserProfile = null;
            }

            ViewModelLocator.Main.LastPostOpened = CurrentPostViewModel.Post;
            var user = await Pr0grammService.LoadUserProfileAsync(CurrentPostViewModel.Post.User);
            ViewModelLocator.Main.UserProfile = user;

            DeInitPostPage(new CancelEventArgs());
            var postContainer = GetPostContainer();
            postContainer.DeInitContainer(this, new CancelEventArgs());

            ViewModelLocator.Navigation.NavigateTo(PageUrls.UserProfilePageUrl);

            if (ViewModelLocator.Main.UserProfileNavigationHistory)
            {
                ViewModelLocator.Navigation.ClearBackstack(true);
            }
        }

        private string gifString = "<HTML><HEAD><meta name='viewport' content='width=device-width, initial-scale=1.0' /></HEAD><BODY bgcolor='REPLACECOLOR'><IMG src='GIFURL'></BODY></HTML>";
        private void BrowserPanel_OnTap(object sender, GestureEventArgs e)
        {
            var imagePath = CurrentPostViewModel.Post.Image;
            if (imagePath.EndsWith(".gif"))
            {
                var colorToUse = "#000000";
                if (ViewModelLocator.Settings.Theme == "Light Theme")
                {
                    colorToUse = "#FFFFFF";
                }

                var gifUrl = gifString.Replace("GIFURL", imagePath).Replace("REPLACECOLOR", colorToUse);

                this.BigBrowser.Visibility = Visibility.Visible;
                this.BigBrowser.NavigateToString(gifUrl);
                bigBrowserVisible = true;
                return;
            }

            if (imagePath.EndsWith(".webm"))
            {
                return;
            }

            var container = GetPostContainer();
            if (container.slideShowActive)
            {
                this.progressBar1.Visibility = Visibility.Visible;
            }

            ZoomImage();
        }
    }
}