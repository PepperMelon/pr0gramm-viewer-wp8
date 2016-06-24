using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Model
{
    using System.IO.IsolatedStorage;

    public class Settings
    {
        public string LoadSfw
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADSFW"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADSFW"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADSFW"] = value;
            }
        }

        public string LoadNsfw
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADNSFW"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADNSFW"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADNSFW"] = value;
            }
        }

        public string LoadNsfl
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADNSFl"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADNSFl"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADNSFl"] = value;
            }
        }

        public string FagFilter
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("FAGFILTER"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["FAGFILTER"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["FAGFILTER"] = value;
            }
        }

        public string PostView
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("POSTVIEW"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["POSTVIEW"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["POSTVIEW"] = value;
            }
        }

        public string CanLoadPictures
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADPICTURES"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADPICTURES"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADPICTURES"] = value;
            }
        }

        public string CanLoadGifs
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADGIFS"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADGIFS"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADGIFS"] = value;
            }
        }

        public string CanLoadWebms
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADWEBMS"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADWEBMS"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADWEBMS"] = value;
            }
        }

        public string OnlyPositivePosts
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("ONLYPOSITIVEPOSTS"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["ONLYPOSITIVEPOSTS"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["ONLYPOSITIVEPOSTS"] = value;
            }
        }

        public string PostPoints
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("POSTPOINTS"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["POSTPOINTS"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["POSTPOINTS"] = value;
            }
        }

        public string ShowSlideshowButton
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SHOWSLIDESHOWBUTTON"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["SHOWSLIDESHOWBUTTON"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["SHOWSLIDESHOWBUTTON"] = value;
            }
        }

        public string SlideShowSecondsToChange
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SLIDESHOWSECONDSTOCHANGE"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["SLIDESHOWSECONDSTOCHANGE"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["SLIDESHOWSECONDSTOCHANGE"] = value;
            }
        }

        public string AppLang
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("APPLANG"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["APPLANG"] as string;
                }
                else
                {
                    return "de-DE";
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["APPLANG"] = value;
            }
        }

        public string Theme
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("THEME"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["THEME"] as string;
                }
                else
                {
                    return "Dark Theme";
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["THEME"] = value;
            }
        }

        public int? LoadedNews
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("LOADEDNEWS"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["LOADEDNEWS"] as int?;
                }
                else
                {
                    return 0;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["LOADEDNEWS"] = value;
            }
        }

        public bool SettingsChanged { get; set; }

        public bool PostViewChanged { get; set; }

        public bool ThemeChanged { get; set; }

        public string ShouldShowNavigationButtons
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("SHOULDSHOWNAVIGATIONBUTTONS"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["SHOULDSHOWNAVIGATIONBUTTONS"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["SHOULDSHOWNAVIGATIONBUTTONS"] = value;
            }
        }

        public string ManualCommentsLoading
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains("MANUALCOMMENTSLOADING"))
                {
                    return IsolatedStorageSettings.ApplicationSettings["MANUALCOMMENTSLOADING"] as string;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                IsolatedStorageSettings.ApplicationSettings["MANUALCOMMENTSLOADING"] = value;
            }
        }
    }
}