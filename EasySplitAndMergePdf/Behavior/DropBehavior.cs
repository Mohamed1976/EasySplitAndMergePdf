using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using EasySplitAndMergePdf.Interface;
using EasySplitAndMergePdf.Helper;

namespace EasySplitAndMergePdf.Behavior
{
    public class DropBehavior : Behavior<DataGrid>
    {
        #region AllowMultipleFiles

        public static readonly DependencyProperty AllowMultipleFilesProperty =
            DependencyProperty.RegisterAttached("AllowMultipleFiles", typeof(bool),
            typeof(DropBehavior), new FrameworkPropertyMetadata(false));

        public bool AllowMultipleFiles
        {
            get
            {
                return (bool)GetValue(AllowMultipleFilesProperty);
            }
            set
            {
                SetValue(AllowMultipleFilesProperty, value);
            }
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            this.AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            this.AssociatedObject.DragOver += AssociatedObject_DragOver;
            this.AssociatedObject.Drop += AssociatedObject_Drop;
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.DragEnter -= AssociatedObject_DragEnter;
            this.AssociatedObject.DragLeave -= AssociatedObject_DragLeave;
            this.AssociatedObject.DragOver -= AssociatedObject_DragOver;
            this.AssociatedObject.Drop -= AssociatedObject_Drop;

            base.OnDetaching();
        }

        void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            IDropable target = this.AssociatedObject.DataContext as IDropable;
            int index = -1;
            if (target != null)
            {
                if (((DataGrid)sender) != null) { index = ((DataGrid)sender).SelectedIndex; }
                target.Drop(e.Data.GetData(DataFormats.FileDrop), index);
            }
            e.Handled = true;
        }

        void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
            if (DropAllowed(e) == DragDropEffects.Copy)
            {
                var row = UIHelpers.TryFindFromPoint<DataGridRow>((DataGrid)sender, 
                    e.GetPosition(AssociatedObject));
                if (row != null) { ((DataGridRow)row).IsSelected = true; }
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }

        void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DropAllowed(e);
            e.Handled = true;
        }

        private DragDropEffects DropAllowed(DragEventArgs e)
        {
            DragDropEffects dragDropEffects = DragDropEffects.None;

            if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                string[] Dropfiles = (string[])e.Data.GetData(DataFormats.FileDrop);
                if ((AllowMultipleFiles && Dropfiles.Length > 0) ||
                    (!AllowMultipleFiles && Dropfiles.Length == 1))
                {
                    int fileCnt = 0;
                    dragDropEffects = DragDropEffects.Copy;
                    do
                    {
                        if (string.Compare(System.IO.Path.GetExtension(Dropfiles[fileCnt]).ToLower(), ".pdf") != 0)
                            dragDropEffects = DragDropEffects.None;

                    } while (++fileCnt < Dropfiles.Length &&
                        dragDropEffects == DragDropEffects.Copy);
                }
            }

            return dragDropEffects;
        }
    }
}
