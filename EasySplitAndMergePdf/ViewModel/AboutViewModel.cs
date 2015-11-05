using System;
using System.Windows;
using System.Reflection;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Interface;

namespace EasySplitAndMergePdf.ViewModel
{
    public class AboutViewModel : ViewModelBase, ITabViewModel
    {
        #region [ Constructors ]

        public AboutViewModel(string header)
        {
            Header = header;
            InitializeProperties();
        }

        #endregion

        #region [ Properties ]

        private void InitializeProperties()
        {
            StatusBarIsVisible = Visibility.Hidden;
            Assembly assembly = Assembly.GetExecutingAssembly();

            object[] titleAttr = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
            if (titleAttr.Length > 0)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)titleAttr[0];
                if (!string.IsNullOrEmpty(titleAttribute.Title))
                {
                    Title = String.Format("About {0}", titleAttribute.Title);
                }
            }

            object[] productAttr = assembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
            if (productAttr.Length > 0)
            {
                AssemblyProductAttribute productAttribute = (AssemblyProductAttribute)productAttr[0];
                if (!string.IsNullOrEmpty(productAttribute.Product))
                {
                    ProductName = productAttribute.Product;
                }
            }

            Version = assembly.GetName().Version.ToString();

            object[] copyrightAttr = assembly.GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
            if (copyrightAttr.Length > 0)
            {
                AssemblyCopyrightAttribute copyrightAttribute = (AssemblyCopyrightAttribute)copyrightAttr[0];
                if (!string.IsNullOrEmpty(copyrightAttribute.Copyright))
                {
                    Copyright = copyrightAttribute.Copyright;
                }
            }

            object[] descriptionAttr = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (descriptionAttr.Length > 0)
            {
                AssemblyDescriptionAttribute descriptionAttribute = (AssemblyDescriptionAttribute)descriptionAttr[0];
                if (!string.IsNullOrEmpty(descriptionAttribute.Description))
                {
                    Description = descriptionAttribute.Description;
                }
            }

            object[] companyAttr = assembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
            if (companyAttr.Length > 0)
            {
                AssemblyCompanyAttribute companyAttribute = (AssemblyCompanyAttribute)companyAttr[0];
                if (!string.IsNullOrEmpty(companyAttribute.Company))
                {
                    companyName = companyAttribute.Company;
                }
            }

            ITextSharpVersion = iTextSharp.text.Version.GetInstance().GetVersion;
        }

        private string title = string.Empty;
        public string Title
        {
            get { return title; }
            private set { SetProperty(ref title, value); }
        }

        private string productName = string.Empty;
        public string ProductName
        {
            get { return productName; }
            private set { SetProperty(ref productName, value); }
        }

        private string version = string.Empty;
        public string Version
        {
            get { return version; }
            private set { SetProperty(ref version, value); }
        }

        private string copyright = string.Empty;
        public string Copyright
        {
            get { return copyright; }
            private set { SetProperty(ref copyright, value); }
        }

        private string companyName = string.Empty;
        public string CompanyName
        {
            get { return companyName; }
            private set { SetProperty(ref companyName, value); }
        }

        private string description = string.Empty;
        public string Description
        {
            get { return description; }
            private set { SetProperty(ref description, value); }
        }

        private string iTextSharpVersion = string.Empty;
        public string ITextSharpVersion
        {
            get { return iTextSharpVersion; }
            private set { SetProperty(ref iTextSharpVersion, value); }
        }

        #endregion

        #region[ ITabViewModel ]

        private string header = string.Empty;
        public string Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        #endregion
    }
}
