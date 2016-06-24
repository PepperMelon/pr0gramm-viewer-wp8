using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Constants
{
    public static class PageUrls
    {
        public static readonly Uri MainPageUri = new Uri("/MainPage.xaml", UriKind.Relative);
        public static readonly Uri PostPageUri = new Uri("/View/PostPage.xaml", UriKind.Relative);
        public static readonly Uri PostsContainerPageUri = new Uri("/View/PostsContainerPage.xaml", UriKind.Relative);
        public static readonly Uri SettingsPageUrl = new Uri("/View/SettingsPage.xaml", UriKind.Relative);
        public static readonly Uri AboutPageUrl = new Uri("/View/About.xaml", UriKind.Relative);
        public static readonly Uri UserProfilePageUrl = new Uri("/View/UserProfile.xaml", UriKind.Relative);
    }
}
