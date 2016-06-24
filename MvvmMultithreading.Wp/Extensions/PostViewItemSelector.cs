using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Windows;
    using View;
    using ViewModel;

    public class PostViewItemSelector : DataTemplateSelector
    {
        public DataTemplate PostTemplate { get; set; }
        public DataTemplate PostImageTemplate { get; set; }
        public DataTemplate BigImageScrollTemplate { get; set; }
        public DataTemplate SideBySideTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (ViewModelLocator.Settings.PostView == PostView.Effizient.ToString() || ViewModelLocator.Settings.PostView == PostView.BilderOhneRahmen.ToString())
            {
                return SideBySideTemplate;
            }

            if (ViewModelLocator.Settings.PostView == PostView.Grossansicht.ToString())
            {
                return BigImageScrollTemplate;
            }

            if (ViewModelLocator.Settings.PostView == PostView.Bilder.ToString())
            {
                return PostImageTemplate;
            }

            return PostTemplate;
        }
    }
}
