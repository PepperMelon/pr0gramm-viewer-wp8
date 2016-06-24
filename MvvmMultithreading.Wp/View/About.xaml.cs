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
    using System.Reflection;
    using System.Windows.Input;
    using Logic;
    using Microsoft.Phone.Tasks;
    using Resources;
    using ViewModel;

    public partial class About : PhoneApplicationPage
    {
        private List<NewsFeed> newsFeed;

        public About()
        {
            InitializeComponent();
            BuildLocalizedApplicationBar();
            if (ViewModelLocator.Settings.AppLang.Equals("de-DE"))
            {
                this.GermanWhatsNew.Visibility = Visibility.Visible;
                this.EnglishWhatsNew.Visibility = Visibility.Collapsed;
            }
            if (ViewModelLocator.Settings.AppLang.Equals("en-US"))
            {
                this.GermanWhatsNew.Visibility = Visibility.Collapsed;
                this.EnglishWhatsNew.Visibility = Visibility.Visible;
            }
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.IsMenuEnabled = true;
            ApplicationBar.IsVisible = true;

            var searchButton = new ApplicationBarIconButton(new Uri("/Resources/Rateing.png", UriKind.Relative));
            searchButton.Text = AppResources.Rate;
            searchButton.Click += RateAppTap;

            var settingsButton = new ApplicationBarIconButton(new Uri("/Resources/Mail-01.png", UriKind.Relative));
            settingsButton.Text = AppResources.Feedback;
            settingsButton.Click += SendEmailToDeveloper;

            var refreshButton = new ApplicationBarIconButton(new Uri("/Resources/Cash.png", UriKind.Relative));
            refreshButton.Text = AppResources.DonateDeveloper;
            refreshButton.Click += DonateDeveloperTap;

            ApplicationBar.Buttons.Add(searchButton);
            ApplicationBar.Buttons.Add(settingsButton);
            ApplicationBar.Buttons.Add(refreshButton);
        }

        private void RateAppTap(object sender, EventArgs e)
        {
            var marketplaceReviewTask = new MarketplaceReviewTask();
            marketplaceReviewTask.Show();
        }

        private void DonateDeveloperTap(object sender, EventArgs e)
        {
            string url = "";

            string business = "BusinessName"; //einfach die paypal mail addresse rein
            string description = "Donation";
            string country = "DE";
            string currency = "EUR";

            url += "https://www.paypal.com/cgi-bin/webscr" +
                "?cmd=" + "_donations" +
                "&business=" + business +
                "&lc=" + country +
                "&item_name=" + description +
                "&currency_code=" + currency +
                "&bn=" + "PP%2dDonationsBF";

            var webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(url, UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void SendEmailToDeveloper(object sender, EventArgs e)
        {
            var emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "email@adress.com";

            emailComposeTask.Show();
        }


        public string Version
        {
            get
            {
                var asm = Assembly.GetExecutingAssembly();
                var parts = asm.FullName.Split(',');
                return parts[1].Split('=')[1];
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (NavigationContext.QueryString.ContainsKey("item"))
            {
                var index = NavigationContext.QueryString["item"];
                var indexParsed = int.Parse(index);
                aboutPivot.SelectedIndex = indexParsed;
            }
        }

        private async void AboutPivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var pivot = sender as Pivot;
            if (pivot.SelectedIndex == 1)
            {
                if (newsFeed != null)
                {
                    return;
                }

                newsFeed = new List<NewsFeed>();

                try
                {
                    newsFeed = await AzureService.GetNewsFeed();
                    ViewModelLocator.Settings.LoadedNews = newsFeed.Count;
                }
                catch
                {
                }

                news.ItemsSource = newsFeed;
            }
        }
    }
}