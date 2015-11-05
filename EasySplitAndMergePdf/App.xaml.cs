using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using EasySplitAndMergePdf.View;
using EasySplitAndMergePdf.ViewModel;
using EasySplitAndMergePdf.Behavior;

namespace EasySplitAndMergePdf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        #region [ Override Methods ]

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                MainView mainView = new MainView();
                mainView.DataContext = new MainViewModel();
                mainView.Show();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("OnStartup" + ex.ToString());
            }
        }

        #endregion 
    }
}
