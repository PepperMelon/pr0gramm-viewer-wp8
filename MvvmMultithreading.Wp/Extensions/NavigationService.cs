namespace Pr0gramm.Extensions
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Navigation;
    using Microsoft.Phone.Controls;
    using Microsoft.Xna.Framework.Audio;
    using Pr0gramm.Interfaces;

    public class NavigationService : INavigationService
    {
        private PhoneApplicationFrame mainFrame;

        public event NavigatingCancelEventHandler Navigating;

        public Uri CurrentPageUri { get; set; }

        public void NavigateTo(Uri pageUri)
        {
            if (EnsureMainFrame())
            {
                CurrentPageUri = pageUri;
                mainFrame.Navigate(pageUri);
            }
        }

        public void ClearBackstack(bool exceptForOneMain)
        {
            if (mainFrame == null)
            {
                return;
            }

            mainFrame = Application.Current.RootVisual as PhoneApplicationFrame;

            while (mainFrame.BackStack.Any() && IsOneMain(exceptForOneMain))
            {
                mainFrame.RemoveBackEntry();
            }
        }

        private bool IsOneMain(bool exceptForOneMain)
        {
            var isOneMain = !(mainFrame.BackStack.Count() <= 1 &&
                              mainFrame.BackStack.First().Source.OriginalString.EndsWith("MainPage.xaml")) || !exceptForOneMain;
            return isOneMain;
        }

        public void GoBack()
        {
            if (EnsureMainFrame()
                && mainFrame.CanGoBack)
            {
                mainFrame.GoBack();
            }
        }

        private bool EnsureMainFrame()
        {
            if (mainFrame != null)
            {
                return true;
            }

            mainFrame = Application.Current.RootVisual as PhoneApplicationFrame;

            if (mainFrame != null)
            {
                // Could be null if the app runs inside a design tool
                mainFrame.Navigating += (s, e) =>
                {
                    if (Navigating != null)
                    {
                        Navigating(s, e);
                    }
                };

                return true;
            }

            return false;
        }
    }
}
