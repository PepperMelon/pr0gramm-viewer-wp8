using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.ViewModel
{
    using System.Windows;
    using BaseClasses;
    using Model;

    public class CurrentPostViewModel : ExtendedViewModelBase
    {
        private Post post;
        private PostInfo postInfo;
        private double progressValue;

        public Post Post
        {
            get
            {
                return post;
            }
            set
            {
                post = value;
                this.OnPropertyChanged();
            }
        }

        public PostInfo PostInfo
        {
            get
            {
                return postInfo;
            }
            set
            {
                postInfo = value;
                this.OnPropertyChanged();
            }
        }

        public double ProgressValue
        {
            get
            {
                return progressValue;
            }
            set
            {
                progressValue = value;
                this.OnPropertyChanged();
            }
        }

        public double PhoneHeight
        {
            get
            {
                return Application.Current.Host.Content.ActualHeight;
            }
        }
    }
}
