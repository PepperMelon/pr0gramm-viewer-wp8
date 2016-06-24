namespace Pr0gramm.ViewModel
{
    using BaseClasses;

    public class PostsContainerViewModel : ExtendedViewModelBase
    {
        private CurrentPostViewModel previousPost = new CurrentPostViewModel();
        private CurrentPostViewModel currentPost = new CurrentPostViewModel();
        private CurrentPostViewModel nextPost = new CurrentPostViewModel();
        private bool lockPivot = false;

        public CurrentPostViewModel PreviousPost
        {
            get { return previousPost; }
            set
            {
                previousPost = value;
                this.OnPropertyChanged();
            }
        }

        public CurrentPostViewModel CurrentPost
        {
            get { return currentPost; }
            set
            {
                currentPost = value;
                this.OnPropertyChanged();
            }
        }

        public CurrentPostViewModel NextPost
        {
            get { return nextPost; }
            set
            {
                nextPost = value;
                this.OnPropertyChanged();
            }
        }
    }
}