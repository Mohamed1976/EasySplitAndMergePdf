using System.Windows;
using System.Collections.Generic;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Command;

namespace EasySplitAndMergePdf.ViewModel
{
    public class FilePropertiesViewModel : ViewModelBase
    {
        #region [ Constructors ]

        public FilePropertiesViewModel(string fileName,
            Dictionary<string, string> properties,
            Dictionary<string, string> info,
            Dictionary<string, string> security)
        {
            FileName = fileName;
            Properties = properties;
            Info = info;
            Security = security;
            InitializeCommands();
        }

        #endregion

        #region [ Commands ]

        private void InitializeCommands()
        {
            CloseWindowCmd = new RelayCommand<object>(OnCloseWindowCmdExecute, OnCloseWindowCmdCanExecute);
        }

        public RelayCommand<object> CloseWindowCmd { get; private set; }

        public bool OnCloseWindowCmdCanExecute(object parameter)
        {
            return (parameter as Window) != null;
        }

        public void OnCloseWindowCmdExecute(object parameter)
        {
            Window window = (Window)parameter;
            SystemCommands.CloseWindow(window);

            if (window.Owner != null)
            {
                window.Owner.Focus();
            }
        }

        #endregion

        #region [ Properties ]

        private string fileName = string.Empty;
        public string FileName
        {
            get { return fileName; }
            private set { SetProperty(ref fileName, value); }
        }

        private Dictionary<string, string> properties = null;
        public Dictionary<string, string> Properties
        {
            get { return properties; }
            private set { SetProperty(ref properties, value); }
        }

        private Dictionary<string, string> info = null;
        public Dictionary<string, string> Info
        {
            get { return info; }
            private set { SetProperty(ref info, value); }
        }

        private Dictionary<string, string> security = null;
        public Dictionary<string, string> Security
        {
            get { return security; }
            private set { SetProperty(ref security, value); }
        }

        #endregion
    }
}
