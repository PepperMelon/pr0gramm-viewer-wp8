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
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using Constants;
    using Logic;
    using Model;
    using Resources;
    using ViewModel;

    public partial class UserProfle : PhoneApplicationPage
    {
        public UserProfle()
        {
            InitializeComponent();
            ViewModelLocator.Main.UserProfileCollection = false;
            this.BackKeyPress += OnBackKeyPress;
            this.Loaded += OnLoaded;
        }

        private void OnBackKeyPress(object sender, CancelEventArgs e)
        {
            ViewModelLocator.Main.UserProfile.PostLikes.Clear();
            ViewModelLocator.Main.UserProfile.PostUploads.Clear();
            ViewModelLocator.Main.UserProfile = null;

            if (ViewModelLocator.Main.UserProfileNavigationHistory)
            {
                ViewModelLocator.Main.UserProfileNavigationHistory = false;
                ViewModelLocator.Navigation.ClearBackstack(true);
                return;
            }

            if (!ViewModelLocator.Main.UserProfileCollection)
            {
                ViewModelLocator.PostContainerViewModel.PreviousPost.Post = ViewModelLocator.Main.LastPostOpened;
                ViewModelLocator.Navigation.NavigateTo(PageUrls.PostsContainerPageUri);
                ViewModelLocator.Navigation.ClearBackstack(true);
            }
            else
            {
                ViewModelLocator.Main.UserProfileCollection = false;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            profile().LoadLikePosts = () => Pr0grammService.LoadUserPosts(ViewModelLocator.Main.UserProfile.User.Name, profile().PostLikes, ApiCalls.SelfUserLikes, ViewModelLocator.Main.UserProfile.LikeCount);
            profile().LoadUploadPosts = () => Pr0grammService.LoadUserPosts(ViewModelLocator.Main.UserProfile.User.Name, profile().PostUploads, ApiCalls.UserUploads, ViewModelLocator.Main.UserProfile.UploadCount);
        }

        private void OpenPost(object sender, GestureEventArgs e)
        {
            var post = (sender as Grid).DataContext as Post;
            if (post == null)
            {
                return;
            }

            var selectedItemHeader = (this.PivotElement.SelectedItem as PivotItem).Header;
            if (selectedItemHeader.Equals(AppResources.Favorites))
            {
                ViewModelLocator.Main.ListToUse = profile().PostLikes;
                ViewModelLocator.Main.ActionForReloadingPost = () => Pr0grammService.LoadUserPosts(ViewModelLocator.Main.UserProfile.User.Name, profile().PostLikes, ApiCalls.SelfUserLikes, ViewModelLocator.Main.UserProfile.LikeCount);
            }
            if (selectedItemHeader.Equals(AppResources.Uploads))
            {
                ViewModelLocator.Main.ListToUse = profile().PostUploads;
                ViewModelLocator.Main.ActionForReloadingPost = () => Pr0grammService.LoadUserPosts(ViewModelLocator.Main.UserProfile.User.Name, profile().PostUploads, ApiCalls.UserUploads, ViewModelLocator.Main.UserProfile.UploadCount);
                //ViewModelLocator.Main.UserContentType = UserContentType.Uploads;
            }

            ViewModelLocator.PostContainerViewModel.PreviousPost.Post = post;
            ViewModelLocator.Main.UserProfileNavigationHistory = true;
            ViewModelLocator.Main.UserProfileCollection = true;
            ViewModelLocator.Navigation.ClearBackstack(true);
            ViewModelLocator.Navigation.NavigateTo(PageUrls.PostsContainerPageUri);
        }

        private void PivotItemChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.SelectedIndex == 1)
            {
                if (!ViewModelLocator.Main.UserProfile.PostUploads.Any())
                {
                    Pr0grammService.LoadUserPosts(ViewModelLocator.Main.UserProfile.User.Name, profile().PostUploads, ApiCalls.UserUploads, ViewModelLocator.Main.UserProfile.UploadCount);
                }
            }
            else if (pivot.SelectedIndex == 2)
            {
                if (!ViewModelLocator.Main.UserProfile.PostLikes.Any())
                {
                    Pr0grammService.LoadUserPosts(ViewModelLocator.Main.UserProfile.User.Name, profile().PostLikes, ApiCalls.SelfUserLikes, ViewModelLocator.Main.UserProfile.LikeCount);
                }
            }
        }

        private UserProfile profile()
        {
            return this.DataContext as UserProfile;
        }

        // This is a fix needed to avoid layoutcycle. This occured because when too few images were loaded and didn't occupy enough
        // visible space the exception was thrown.
        private void ImgGameList_OnImageOpened(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            
            if (PivotElement.SelectedIndex == 1 && profile().UploadCount < 3)
            {
                var bitmapImage = image.Source as BitmapImage;
                image.Height = bitmapImage.PixelHeight;
                image.Stretch = Stretch.Uniform;
                image.UpdateLayout();
                return;
            }

            if (PivotElement.SelectedIndex == 2 && profile().LikeCount < 3)
            {
                var bitmapImage = image.Source as BitmapImage;
                image.Height = bitmapImage.PixelHeight;
                image.Stretch = Stretch.Uniform;
                image.UpdateLayout();
            }
        }
    }
}