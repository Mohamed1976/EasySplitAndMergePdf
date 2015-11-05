using System.Windows;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Command;
using EasySplitAndMergePdf.Model;

namespace EasySplitAndMergePdf.ViewModel
{
    public class PdfLoginViewModel : ViewModelBase
    {
        #region [ Defines ]

        private Window pdfLoginView = null;

        #endregion

        #region [ Constructors ]

        public PdfLoginViewModel(Window pdfLoginWindow, PdfFile lockedFile)
        {
            pdfLoginView = pdfLoginWindow;
            LockedFile = lockedFile;
            InitializeCommands();
        }

        #endregion

        #region [ Command properties ]

        private void InitializeCommands()
        {
            OkCmd = new RelayCommand<object>(OnOkCmdExecute, OnOkCmdCanExecute);
            CancelCmd = new RelayCommand(OnCancelCmdExecute);
        }

        public RelayCommand<object> OkCmd { get; private set; }
        public RelayCommand CancelCmd { get; private set; }

        private void OnOkCmdExecute(object parameter)
        {
            if (LockedFile.Open(((System.Windows.Controls.PasswordBox)parameter).Password) == Define.Success)
            {
                ((System.Windows.Controls.PasswordBox)parameter).Password = string.Empty;
                HasError = false;
                ErrorContent = string.Empty;
                pdfLoginView.Owner.Focus();
                pdfLoginView.DialogResult = true;
                pdfLoginView.Close();
            }
            else
            {
                ((System.Windows.Controls.PasswordBox)parameter).Password = string.Empty;
                ErrorContent = "Failed to unlock PDF file, please try again.";
                HasError = true;
            }
        }

        private bool OnOkCmdCanExecute(object parameter)
        {
            return ((System.Windows.Controls.PasswordBox)parameter).Password.Length > 0;
        }

        private void OnCancelCmdExecute()
        {
            pdfLoginView.DialogResult = false;
            pdfLoginView.Close();
        }

        #endregion

        #region [ Properties ]

        private PdfFile lockedFile = null;
        public PdfFile LockedFile
        {
            get { return lockedFile; }
            private set { SetProperty(ref lockedFile, value); }
        }

        private string errorContent = string.Empty;
        public string ErrorContent
        {
            get { return errorContent; }
            private set { SetProperty(ref errorContent, value); }
        }

        private bool hasError = false;
        public bool HasError
        {
            get { return hasError; }
            private set { SetProperty(ref hasError, value); }
        }

        #endregion
    }
}
