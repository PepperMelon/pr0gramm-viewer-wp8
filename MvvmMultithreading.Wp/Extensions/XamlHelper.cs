using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Extensions
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public static class XamlHelper
    {
        public static List<Control> AllChildren(DependencyObject parent)
        {
            var _List = new List<Control>();
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var _Child = VisualTreeHelper.GetChild(parent, i);
                if (_Child is Control)
                {
                    _List.Add(_Child as Control);
                }

                _List.AddRange(AllChildren(_Child));
            }

            return _List;
        }

        public static childItem FindVisualChild<childItem>(DependencyObject obj) where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

        public static T FindControl<T>(DependencyObject parentContainer, string controlName)
        {
            var childControls = AllChildren(parentContainer);
            var control = childControls.OfType<Control>().Where(x => x.Name.Equals(controlName)).Cast<T>().First();
            return control;
        }
    }
}
