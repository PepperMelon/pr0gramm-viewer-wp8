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
    using System.Windows.Input;
    using Coding4Fun.Toolkit.Controls;
    using Constants;
    using Extensions;
    using Resources;
    using ViewModel;

    public partial class SettingsPage : PhoneApplicationPage
    {
        private static bool loadingSettings = false;

        public SettingsPage()
        {
            loadingSettings = true;
            InitializeComponent();
            this.BackKeyPress += OnBackKeyPress;

            this.FagFilterList.ItemsSource = Enum.GetValues(typeof(FagFilter));

            this.PostsViewSetting.ItemsSource = Enum.GetValues(typeof(PostView));

            var languages = new List<string>() { AppResources.German, AppResources.English };
            this.LanguageSettings.ItemsSource = languages;

            if (ViewModelLocator.Settings.AppLang.Equals("de-DE"))
            {
                LanguageSettings.SelectedItem = AppResources.German;
            }
            if (ViewModelLocator.Settings.AppLang.Equals("en-US"))
            {
                LanguageSettings.SelectedItem = AppResources.English;
            }

            this.ThemeSettings.ItemsSource = new List<string>() { "Dark Theme", "Light Theme" };

            if (ViewModelLocator.Settings.Theme.Equals("Dark Theme"))
            {
                ThemeSettings.SelectedItem = "Dark Theme";
            }
            if (ViewModelLocator.Settings.Theme.Equals("Light Theme"))
            {
                ThemeSettings.SelectedItem = "Light Theme";
            }

            if (string.IsNullOrEmpty(ViewModelLocator.Settings.LoadSfw))
            {
                this.SfwSwitch.IsChecked = true;
                ViewModelLocator.Settings.LoadSfw = "true";
                ViewModelLocator.Settings.SettingsChanged = true;
            }

            this.SfwSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.LoadSfw);
            this.NSFWSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.LoadNsfw);
            this.NSFLSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.LoadNsfl);
            this.ShowNavigationButtonsSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.ShouldShowNavigationButtons);
            this.ManualCommentsSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.ManualCommentsLoading);

            if (!string.IsNullOrEmpty(ViewModelLocator.Settings.FagFilter))
            {
                var fagFilter = Enum.Parse(typeof(FagFilter), ViewModelLocator.Settings.FagFilter);
                this.FagFilterList.SelectedItem = fagFilter;
            }

            if (!string.IsNullOrEmpty(ViewModelLocator.Settings.PostView))
            {
                var postView = Enum.Parse(typeof(PostView), ViewModelLocator.Settings.PostView);
                this.PostsViewSetting.SelectedItem = postView;
            }

            this.PicSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.CanLoadPictures);
            this.GifSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.CanLoadGifs);
            this.WebmSwitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.CanLoadWebms);
            this.Positivewitch.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.OnlyPositivePosts);
            this.ActivateSlideshow.IsChecked = Helper.StringToBoolean(ViewModelLocator.Settings.ShowSlideshowButton);
            loadingSettings = false;
        }

        private void OnBackKeyPress(object sender, CancelEventArgs cancelEventArgs)
        {
            loadingSettings = false;
            if (ViewModelLocator.Settings.PostPoints != XamlHelper.FindVisualChild<TextBox>(this.PointsSetter).Text)
            {
                ViewModelLocator.Settings.PostPoints = XamlHelper.FindVisualChild<TextBox>(this.PointsSetter).Text;
                ViewModelLocator.Settings.SettingsChanged = true;
            }

            if (ViewModelLocator.Settings.SlideShowSecondsToChange != XamlHelper.FindVisualChild<TextBox>(this.SlideShowSeconds).Text)
            {
                ViewModelLocator.Settings.SlideShowSecondsToChange = XamlHelper.FindVisualChild<TextBox>(this.SlideShowSeconds).Text;
                ViewModelLocator.Settings.SettingsChanged = true;
            }
        }

        private void FagFilterChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListPicker).SelectedItem;
            if (item != null)
            {
                var selectedItem = item.ToString();
                SetFagFilter(selectedItem);
            }
        }

        private void PostViewChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListPicker).SelectedItem;
            if (item != null)
            {
                var selectedItem = item.ToString();
                SetPostView(selectedItem);
            }
        }

        private static void SetFagFilter(string selectedItem)
        {
            if (ViewModelLocator.Settings.FagFilter == selectedItem)
            {
                return;
            }

            ViewModelLocator.Settings.FagFilter = selectedItem;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private static void SetPostView(string selectedItem)
        {
            if (loadingSettings)
            {
                return;
            }

            if (selectedItem.Equals("List"))
            {
                selectedItem = "Liste";
            }
            if (selectedItem.Equals("Pictures"))
            {
                selectedItem = "Bilder";
            }

            if (ViewModelLocator.Settings.PostView == selectedItem)
            {
                return;
            }

            ViewModelLocator.Settings.PostView = selectedItem;
            ViewModelLocator.Settings.PostViewChanged = true;
        }

        private void ShouldLoadPicsCheck(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.CanLoadPictures = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void ShouldLoadGifsCheck(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.CanLoadGifs = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void ShouldLoadWebmsCheck(object sender, RoutedEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.CanLoadWebms = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void ShouldLoadPositivePostCheck(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.OnlyPositivePosts = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void ShouldLoadSFWCheck(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.LoadSfw = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void ShouldLoadNSFWCheck(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.LoadNsfw = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void ShouldLoadNSFLCheck(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.LoadNsfl = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void PostPointsTextBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var points = "0";
            if (!string.IsNullOrEmpty(ViewModelLocator.Settings.PostPoints))
            {
                points = ViewModelLocator.Settings.PostPoints;
            }

            var textBox = sender as TextBox;
            textBox.Text = points;
        }

        private void SlideShowSecondsTextbox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var seconds = "10";
            if (!string.IsNullOrEmpty(ViewModelLocator.Settings.SlideShowSecondsToChange))
            {
                seconds = ViewModelLocator.Settings.SlideShowSecondsToChange;
            }

            var textBox = sender as TextBox;
            textBox.Text = seconds;
        }

        private void ActivateSlideShowSwitch(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.ShowSlideshowButton = booleanToString;
            ViewModelLocator.Settings.SettingsChanged = true;
        }

        private void LanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListPicker).SelectedItem;
            if (item != null)
            {
                var selectedItem = item.ToString();
                if (selectedItem.Equals(AppResources.English) && ViewModelLocator.Settings.AppLang != "en-US" && !loadingSettings)
                {
                    ViewModelLocator.Settings.AppLang = "en-US";
                    ViewModelLocator.ShowNotification(AppResources.RestartAppToTakeEffect, string.Empty);
                }
                if (selectedItem.Equals(AppResources.German) && ViewModelLocator.Settings.AppLang != "de-DE" && !loadingSettings)
                {
                    ViewModelLocator.Settings.AppLang = "de-DE";
                    ViewModelLocator.ShowNotification(AppResources.RestartAppToTakeEffect, string.Empty);
                }
            }
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (sender as ListPicker).SelectedItem;
            if (item != null)
            {
                var selectedItem = item.ToString();
                if (selectedItem.Equals("Dark Theme") && ViewModelLocator.Settings.Theme != "Dark Theme" && !loadingSettings)
                {
                    ViewModelLocator.Settings.Theme = "Dark Theme";
                }
                if (selectedItem.Equals("Light Theme") && ViewModelLocator.Settings.Theme != "Light Theme" && !loadingSettings)
                {
                    ViewModelLocator.Settings.Theme = "Light Theme";
                }
            }

            ViewModelLocator.Settings.ThemeChanged = true;
        }

        private void ShouldShowNavigationButtons(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.ShouldShowNavigationButtons = booleanToString;
        }

        private void ShouldEnableManualComments(object sender, GestureEventArgs e)
        {
            var toggleSwitch = sender as ToggleSwitch;
            var booleanToString = (bool)toggleSwitch.IsChecked ? "true" : "false";
            ViewModelLocator.Settings.ManualCommentsLoading = booleanToString;
        }
    }

    public enum PostView
    {
        [LocalizedDescription("List", typeof(AppResources))]
        Liste,

        [LocalizedDescription("Pictures", typeof(AppResources))]
        Bilder,

        [LocalizedDescription("PicturesWithoutBorder", typeof(AppResources))]
        BilderOhneRahmen,

        [LocalizedDescription("Efficient", typeof(AppResources))]
        Effizient,

        [LocalizedDescription("BigView", typeof(AppResources))]
        Grossansicht


    }
}