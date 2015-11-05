using System;
using System.Windows;
using System.Windows.Controls;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Command;

namespace EasySplitAndMergePdf.ViewModel
{
    public class WebBrowserViewModel : ViewModelBase
    {
        #region[ Constructors ]

        public WebBrowserViewModel(string filePath)
        {
            FileName = filePath;
            BrowserUri = new Uri(filePath);
            InitializeCommands();
        }

        #endregion

        #region [ Commands]

        public RelayCommand<object> CleanUpCmd { get; private set; }
        public RelayCommand<object> CloseCmd { get; private set; }

        private void InitializeCommands()
        {
            CleanUpCmd = new RelayCommand<object>(OnCleanUpCmdExecute);
            CloseCmd = new RelayCommand<object>(OnCloseCmdExecute, OnCloseCmdCanExecute);
        }

        //Avoid PDF lock by AcroRd32.dll after closure
        private void OnCleanUpCmdExecute(object parameter)
        {
            WebBrowser webBrowser = (WebBrowser)parameter;
            if (webBrowser != null)
            {
                App.Current.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    webBrowser.NavigateToString("about:blank");
                }));
            }
        }

        private bool OnCloseCmdCanExecute(object parameter)
        {
            return (parameter as Window) != null;
        }

        private void OnCloseCmdExecute(object parameter)
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

        private Uri browserUri = null;
        public Uri BrowserUri
        {
            get { return browserUri; }
            private set { SetProperty(ref browserUri, value); }
        }

        private string fileName = string.Empty;
        public string FileName
        {
            get { return fileName; }
            private set { SetProperty(ref fileName, value); }
        }

        #endregion
    }
}
