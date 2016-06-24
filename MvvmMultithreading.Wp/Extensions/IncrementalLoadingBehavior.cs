namespace Pr0gramm.Extensions
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Interactivity;
    using Microsoft.Phone.Controls;

    public class IncrementalLoadingBehavior : Behavior<LongListSelector>
    {
        public static readonly DependencyProperty LoadCommandProperty =
            DependencyProperty.Register("LoadCommand",
                typeof(ICommand),
                typeof(IncrementalLoadingBehavior),
                new PropertyMetadata(default(ICommand)));

        public ICommand LoadCommand
        {
            get { return (ICommand)GetValue(LoadCommandProperty); }
            set { SetValue(LoadCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ItemRealized += OnItemRealized;
        }

        private void OnItemRealized(object sender, ItemRealizationEventArgs e)
        {
            var longListSelector = sender as LongListSelector;
            if (longListSelector == null)
            {
                return;
            }

            var item = e.Container.Content;
            var items = longListSelector.ItemsSource;
            var index = items.IndexOf(item);

            if ((items.Count - index <= 1) && (LoadCommand != null) && (LoadCommand.CanExecute(null)))
            {
                LoadCommand.Execute(null);
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.ItemRealized -= OnItemRealized;
        }
    }
}
