using System.Collections.ObjectModel;
using System.Linq;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Interface;

namespace EasySplitAndMergePdf.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region [ Constructors ]

        public MainViewModel()
        {
            InitializeProperties();
        }

        #endregion

        #region [ Properties ]

        private void InitializeProperties()
        {
            TabControls = new ObservableCollection<ITabViewModel>();
            TabControls.Add(new SplitPdfViewModel("Split"));
            TabControls.Add(new MergePdfViewModel("Merge"));
            TabControls.Add(new AboutViewModel("About"));
            SelectedTab = TabControls.First();
        }

        private ITabViewModel selectedTab = null;
        public ITabViewModel SelectedTab
        {
            get { return selectedTab; }
            set { SetProperty(ref selectedTab, value); }
        }

        private ObservableCollection<ITabViewModel> tabControls = null;
        public ObservableCollection<ITabViewModel> TabControls
        {
            get { return tabControls; }
            private set { SetProperty(ref tabControls, value); }
        }

        #endregion
    }
}
