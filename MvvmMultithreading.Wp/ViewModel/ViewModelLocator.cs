namespace Pr0gramm.ViewModel
{
    using System.Collections.Generic;
    using System.Threading;
    using Coding4Fun.Toolkit.Controls;
    using Extensions;
    using GalaSoft.MvvmLight.Ioc;
    using Interfaces;
    using Microsoft.Practices.ServiceLocation;
    using Model;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        private static VoteObject voteObject;

        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<PostsContainerViewModel>();
            SimpleIoc.Default.Register<Settings>();
            SimpleIoc.Default.Register<AuthObj>();
            SimpleIoc.Default.Register<VoteItemViewModel>();
            SimpleIoc.Default.Register<INavigationService, NavigationService>();
        }

        public static MainViewModel Main
        {
            get
            {
                return ServiceLocator.Current.GetInstance<MainViewModel>();
            }
        }

        public static PostsContainerViewModel PostContainerViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<PostsContainerViewModel>();
            }
        }

        public static Settings Settings
        {
            get
            {
                return ServiceLocator.Current.GetInstance<Settings>();
            }
        }

        public static AuthObj Authentication
        {
            get
            {
                return ServiceLocator.Current.GetInstance<AuthObj>();
            }
        }

        public static VoteItemViewModel VoteItemViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<VoteItemViewModel>();
            }
        }

        public static VoteObject VoteObject
        {
            get
            {
                if (voteObject == null)
                {
                    VoteObject = new VoteObject();
                }

                return voteObject;
            }
            set
            {
                voteObject = value;
            }
        }

        public static INavigationService Navigation
        {
            get
            {
                return ServiceLocator.Current.GetInstance<INavigationService>();
            }
        }

        public static void ShowNotification(string notification, string title)
        {
            var toast = new ToastPrompt();
            toast.FontSize = 20;
            toast.Title = title;
            toast.Message = notification;
            toast.TextOrientation = System.Windows.Controls.Orientation.Horizontal;
            toast.Show();
        }

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}