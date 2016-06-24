namespace Pr0gramm
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Constants;
    using Extensions;
    using Logic;
    using Microsoft.Phone.Controls;
    using Microsoft.Phone.Shell;
    using Model;
    using Resources;
    using ViewModel;
    using GestureEventArgs = System.Windows.Input.GestureEventArgs;

    public partial class MainPage : PhoneApplicationPage
    {
        private CustomMessageBox customMessageBox;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            this.Loaded += OnLoaded;
            this.InboxInfo.Visibility = Visibility.Collapsed;
            ViewModelLocator.Main.Category = Category.Hot;
            Init();
            Pr0grammService.AuthenticationCompleted += Pr0GrammServiceOnAuthenticationCompleted;
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;

            var searchButton = new ApplicationBarIconButton(new Uri("Resources/Search.png", UriKind.Relative));
            searchButton.Text = AppResources.Search;
            searchButton.Click += SearchClick;

            var settingsButton = new ApplicationBarIconButton(new Uri("Resources/config.png", UriKind.Relative));
            settingsButton.Text = AppResources.Settings;
            settingsButton.Click += SettingsClick;

            var refreshButton = new ApplicationBarIconButton(new Uri("Resources/Command-Refresh.png", UriKind.Relative));
            refreshButton.Text = AppResources.Refresh;
            refreshButton.Click += ReloadClick;

            ApplicationBar.Buttons.Add(searchButton);
            ApplicationBar.Buttons.Add(settingsButton);
            ApplicationBar.Buttons.Add(refreshButton);

            var loginMenuItem = new ApplicationBarMenuItem(AppResources.Login);
            loginMenuItem.Click += LoginClick;

            var aboutMenuItem = new ApplicationBarMenuItem(AppResources.About);
            aboutMenuItem.Click += AboutClick;

            ApplicationBar.MenuItems.Add(loginMenuItem);
            ApplicationBar.MenuItems.Add(aboutMenuItem);
        }

        private async void ProfileClick(object sender, EventArgs e)
        {
            var user = await Pr0grammService.LoadUserProfileAsync(ViewModelLocator.Authentication.UserName);
            ViewModelLocator.Main.UserProfile = user;

            ViewModelLocator.Navigation.NavigateTo(PageUrls.UserProfilePageUrl);
            ViewModelLocator.Main.UserProfileNavigationHistory = true;
        }

        private async void Init()
        {
            if (string.IsNullOrEmpty(ViewModelLocator.Settings.CanLoadPictures))
            {
                ViewModelLocator.Settings.CanLoadPictures = "true";
            }
            if (string.IsNullOrEmpty(ViewModelLocator.Settings.CanLoadGifs))
            {
                ViewModelLocator.Settings.CanLoadGifs = "true";
            }
            if (string.IsNullOrEmpty(ViewModelLocator.Settings.CanLoadWebms))
            {
                ViewModelLocator.Settings.CanLoadWebms = "true";
            }

            if (ViewModelLocator.Settings.Theme == "Dark Theme")
            {
                ThemeManager.ToDarkTheme();
            }
            if (ViewModelLocator.Settings.Theme == "Light Theme")
            {
                ThemeManager.ToLightTheme();
            }

            await Pr0grammService.LoadVotes("0");

            try
            {
                var newsFeed = await AzureService.GetNewsFeed();
                if (ViewModelLocator.Settings.LoadedNews != newsFeed.Count)
                {
                    ViewModelLocator.Navigation.NavigateTo(new Uri("/View/About.xaml?item=1", UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            if (ViewModelLocator.Settings.ThemeChanged)
            {
                if (ViewModelLocator.Settings.Theme == "Dark Theme")
                {
                    ThemeManager.ToDarkTheme();
                }
                if (ViewModelLocator.Settings.Theme == "Light Theme")
                {
                    ThemeManager.ToLightTheme();
                }
            }

            if (ViewModelLocator.Settings.SettingsChanged)
            {
                Pr0grammService.ClearPosts();
                ViewModelLocator.Settings.SettingsChanged = false;
                ViewModelLocator.Main.LoadPostsCommand.Execute(null);
            }

            if (ViewModelLocator.Settings.PostViewChanged)
            {
                PostsListSelector.LayoutMode = (LongListSelectorLayoutMode)new LayoutModeConverter().Convert(null, null, null, null);
                NewPostsListSelector.LayoutMode = (LongListSelectorLayoutMode)new LayoutModeConverter().Convert(null, null, null, null);
                PostsListSelector.GridCellSize = (Size)new GridCellSizeConverter().Convert(null, null, null, null);
                NewPostsListSelector.GridCellSize = (Size)new GridCellSizeConverter().Convert(null, null, null, null);
                ViewModelLocator.Settings.PostViewChanged = false;
            }

            var progressIndicator = SystemTray.ProgressIndicator;
            if (progressIndicator != null)
            {
                return;
            }

            progressIndicator = new ProgressIndicator();

            SystemTray.SetProgressIndicator(this, progressIndicator);

            var binding = new Binding("IsLoading") { Source = ViewModelLocator.Main };
            BindingOperations.SetBinding(progressIndicator, ProgressIndicator.IsVisibleProperty, binding);

            binding = new Binding("IsLoading") { Source = ViewModelLocator.Main };
            BindingOperations.SetBinding(progressIndicator, ProgressIndicator.IsIndeterminateProperty, binding);

            progressIndicator.Text = AppResources.LoadingPostsTrace;
        }

        private async void Pr0GrammServiceOnAuthenticationCompleted(object sender, bool firstTimeAuth)
        {
            if (ViewModelLocator.Authentication.IsAuthenticated && firstTimeAuth)
            {
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.LogOut;
                AddProfileMenuItem();
            }

            if (ViewModelLocator.Authentication.IsAuthenticated)
            {
                if (ViewModelLocator.Main.InboxCount <= 0)
                {
                    this.InboxInfo.Visibility = Visibility.Collapsed;
                }
                else
                {
                    this.InboxInfo.Visibility = Visibility.Visible;
                }
            }
        }

        private void AddProfileMenuItem()
        {
            var profileMenuItem = new ApplicationBarMenuItem(AppResources.Profile);
            profileMenuItem.Click += ProfileClick;
            ApplicationBar.MenuItems.Insert(1, profileMenuItem);
        }

        private void OpenPostTap(object sender, GestureEventArgs e)
        {
            var post = (sender as Grid).DataContext as Post;
            ViewModelLocator.Main.ActionForReloadingPost = Pr0grammService.LoadTopPosts;

            if (ViewModelLocator.Main.Category == Category.New)
            {
                ViewModelLocator.Main.ListToUse = ViewModelLocator.Main.NewPosts;
            }
            else if (ViewModelLocator.Main.Category == Category.Hot)
            {
                ViewModelLocator.Main.ListToUse = ViewModelLocator.Main.Posts;
            }

            ViewModelLocator.PostContainerViewModel.PreviousPost.Post = post;
            ViewModelLocator.Navigation.NavigateTo(PageUrls.PostsContainerPageUri);
        }

        private void SettingsClick(object sender, EventArgs e)
        {
            ViewModelLocator.Navigation.NavigateTo(PageUrls.SettingsPageUrl);
        }

        private void PivotItemChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.SelectedIndex == 0)
            {
                ViewModelLocator.Main.Category = Category.Hot;
                if (!ViewModelLocator.Main.Posts.Any())
                {
                    ViewModelLocator.Main.LoadPostsCommand.Execute(null);
                }
            }
            else if (pivot.SelectedIndex == 1)
            {
                ViewModelLocator.Main.Category = Category.New;
                if (!ViewModelLocator.Main.NewPosts.Any())
                {
                    ViewModelLocator.Main.LoadPostsCommand.Execute(null);
                }
            }
        }

        private void SearchClick(object sender, EventArgs e)
        {
            var searchTextBox = new TextBox();
            customMessageBox = new CustomMessageBox()
            {
                Caption = AppResources.SearchEntry,
                Content = searchTextBox,
                LeftButtonContent = "ok",
                RightButtonContent = AppResources.Cancel
            };

            customMessageBox.Dismissed += (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        ViewModelLocator.Main.SearchTag = searchTextBox.Text;
                        Pr0grammService.ClearPosts();
                        ViewModelLocator.Main.LoadPostsCommand.Execute(null);
                        break;
                    case CustomMessageBoxResult.RightButton:
                        // Do something.
                        break;
                    case CustomMessageBoxResult.None:
                        // Do something.
                        break;
                    default:
                        searchTextBox.Text = string.Empty;
                        break;
                }
            };

            customMessageBox.Show();
            searchTextBox.UpdateLayout();
            searchTextBox.Focus();
        }

        private void LoginClick(object sender, EventArgs e)
        {
            if (ViewModelLocator.Authentication.IsAuthenticated)
            {
                ViewModelLocator.Authentication.Clean();
                ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.Login;
                Pr0grammService.DeSubscribeSyncTimer();
                this.InboxInfo.Visibility = Visibility.Collapsed;
                ApplicationBar.MenuItems.RemoveAt(1);
                return;
            }

            var stackPanel = new StackPanel();
            var nameCaption = new TextBlock() { Text = AppResources.Name };
            var nameText = new TextBox();
            var pwCaption = new TextBlock() { Text = AppResources.Password };
            var pwText = new PasswordBox();
            stackPanel.Children.Add(nameCaption);
            stackPanel.Children.Add(nameText);
            stackPanel.Children.Add(pwCaption);
            stackPanel.Children.Add(pwText);

            customMessageBox = new CustomMessageBox()
            {
                Caption = AppResources.LoginData,
                Content = stackPanel,
                LeftButtonContent = "ok",
                RightButtonContent = AppResources.Cancel
            };

            customMessageBox.Dismissed += async (s1, e1) =>
            {
                switch (e1.Result)
                {
                    case CustomMessageBoxResult.LeftButton:
                        await Pr0grammService.AuthenticateAsync(nameText.Text, pwText.Password);
                        if (ViewModelLocator.Authentication.IsAuthenticated)
                        {
                            ((ApplicationBarMenuItem)ApplicationBar.MenuItems[0]).Text = AppResources.LogOut;
                            AddProfileMenuItem();

                            await Pr0grammService.LoadVotes("0");

                            if (ViewModelLocator.Main.InboxCount <= 0)
                            {
                                this.InboxInfo.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                this.InboxInfo.Visibility = Visibility.Visible;
                            }
                        }

                        break;
                    case CustomMessageBoxResult.RightButton:
                        // Do something.
                        break;
                    case CustomMessageBoxResult.None:
                        // Do something.
                        break;
                    default:
                        nameText.Text = string.Empty;
                        break;
                }
            };

            customMessageBox.Show();
            nameText.UpdateLayout();
            nameText.Focus();
        }

        private void AboutClick(object sender, EventArgs e)
        {
            ViewModelLocator.Navigation.NavigateTo(PageUrls.AboutPageUrl);
        }

        private void ReloadClick(object sender, EventArgs e)
        {
            Pr0grammService.ClearPosts();
            ViewModelLocator.Settings.SettingsChanged = false;
            ViewModelLocator.Main.LoadPostsCommand.Execute(null);
        }

        // This is a fix needed to avoid layoutcycle. This occured because when too few images were loaded and didn't occupy enough
        // visible space the exception was thrown.
        private void ImgGameList_OnImageOpened(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;

            if (PivotElement.SelectedIndex == 0 && ViewModelLocator.Main.Posts.Count < 3)
            {
                var bitmapImage = image.Source as BitmapImage;
                image.Height = bitmapImage.PixelHeight;
                image.Stretch = Stretch.Fill;
                image.UpdateLayout();
                return;
            }

            if (PivotElement.SelectedIndex == 1 && ViewModelLocator.Main.NewPosts.Count < 3)
            {
                var bitmapImage = image.Source as BitmapImage;
                image.Height = bitmapImage.PixelHeight;
                image.Stretch = Stretch.Fill;
                image.UpdateLayout();
            }
        }
    }
}