using System;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace EasySplitAndMergePdf.Behavior
{
    class DatagridScrollToViewBehaviors : Behavior<DataGrid>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += 
                new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
            AssociatedObject.IsEnabledChanged += AssociatedObject_IsEnabledChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= 
                new SelectionChangedEventHandler(AssociatedObject_SelectionChanged);
            AssociatedObject.IsEnabledChanged -= AssociatedObject_IsEnabledChanged;
        }

        public void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid)
            {
                DataGrid grid = (sender as DataGrid);
                if (grid.SelectedItem != null)
                {
                    Action action = delegate ()
                    {
                        grid.UpdateLayout();
                        grid.ScrollIntoView(grid.SelectedItem);
                    };
                    grid.Dispatcher.BeginInvoke(action);
                }
            }
        }

        void AssociatedObject_IsEnabledChanged(object sender, 
            System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (sender is DataGrid)
            {
                DataGrid grid = (sender as DataGrid);
                if (grid.SelectedItem != null)
                {
                    Action action = delegate ()
                    {
                        grid.Focus();
                    };
                    grid.Dispatcher.BeginInvoke(action);
                }
            }
        }
    }
}
