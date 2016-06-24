namespace Pr0gramm.Interfaces
{
    using System;
    using System.Windows.Navigation;

    public interface INavigationService
    {
        event NavigatingCancelEventHandler Navigating;
        void NavigateTo(Uri pageUri);
        void GoBack();
        Uri CurrentPageUri { get; set; }
        void ClearBackstack(bool exceptForOneMain);
    }
}
