using System;
using System.Windows;
using System.Windows.Controls;

namespace EasySplitAndMergePdf.Control
{
    public class IntegerTextBox : TextBox
    {
        #region [ Constructors ]

        public IntegerTextBox() : base()
        {
            DataObject.AddPastingHandler(this, new DataObjectPastingEventHandler(CheckPasteFormat));
        }

        #endregion

        #region [ Methods ]

        private bool CheckFormat(string text)
        {
            ushort val;
            return UInt16.TryParse(text, out val);
        }

        private void CheckPasteFormat(object sender, DataObjectPastingEventArgs e)
        {
            var isText = e.SourceDataObject.GetDataPresent(System.Windows.DataFormats.Text, true);
            if (isText)
            {
                var text = e.SourceDataObject.GetData(DataFormats.Text) as string;
                if (CheckFormat(text))
                {
                    return;
                }
            }
            e.CancelCommand();
        }

        #endregion

        #region [ Events ]

        protected override void OnPreviewTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!CheckFormat(e.Text))
            {
                e.Handled = true;
            }
            else
            {
                base.OnPreviewTextInput(e);
            }
        }

        #endregion
    }
}
