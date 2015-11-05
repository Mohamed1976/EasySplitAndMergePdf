using System;
using System.Windows;
using System.Windows.Controls;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using EasySplitAndMergePdf.Model;

namespace EasySplitAndMergePdf.Base
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable, INotifyDataErrorInfo
    {
        #region [ INotifyPropertyChanged Members ]

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Warns the developer if this object does not have a public property with
        /// the specified name. This method does not exist in a Release build.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        private void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                Debug.Fail("Invalid property name: " + propertyName);
            }
        }

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has a new value.</param>

        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);

            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;

            var memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("propertyExpression must represent a valid Member Expression");

            var propertyInfo = memberExpression.Member as System.Reflection.PropertyInfo;
            if (propertyInfo == null)
                throw new ArgumentException("propertyExpression must represent a valid Property on the object");

            handler(this, new PropertyChangedEventArgs(propertyInfo.Name));
        }

        #endregion

        #region [ IDisposable Members ]

        ///<summary>
        /// Invoked when this object is being removed from the application
        /// and will be subject to garbage collection.
        /// </summary>
        public void Dispose()
        {
            OnDispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Child classes can override this method to perform 
        /// clean-up logic, such as removing event handlers.
        /// </summary>
        protected virtual void OnDispose(bool isDisposing)
        {
        }

        ~ViewModelBase()
        {
            OnDispose(false);
        }

        #endregion

        #region [ Overridable properties ]

        private Visibility statusBarIsVisible = Visibility.Visible;
        public virtual Visibility StatusBarIsVisible
        {
            get { return statusBarIsVisible; }
            set
            {
                if (statusBarIsVisible != value)
                {
                    statusBarIsVisible = value;
                    OnPropertyChanged(() => StatusBarIsVisible);
                }
            }
        }

        private bool isBusy = false;
        public virtual bool IsBusy
        {
            get { return isBusy; }
            set
            {
                if (isBusy != value)
                {
                    isBusy = value;
                    OnPropertyChanged(() => IsBusy);
                }
            }
        }

        private bool progressBarIsIndeterminate = false;
        public virtual bool ProgressBarIsIndeterminate
        {
            get { return progressBarIsIndeterminate; }
            set
            {
                if (progressBarIsIndeterminate != value)
                {
                    progressBarIsIndeterminate = value;
                    OnPropertyChanged(() => ProgressBarIsIndeterminate);
                }
            }
        }

        private int progressBarValue = 0;
        public virtual int ProgressBarValue
        {
            get { return progressBarValue; }
            set
            {
                if (progressBarValue != value)
                {
                    progressBarValue = value;
                    OnPropertyChanged(() => ProgressBarValue);
                }
            }
        }

        string progressStatus = string.Empty;
        public virtual string ProgressStatus
        {
            get { return progressStatus; }
            set
            {
                if (progressStatus != value)
                {
                    progressStatus = value;
                    OnPropertyChanged(() => ProgressStatus);
                }
            }
        }

        private ObservableCollection<PdfFile> pdfFiles = null;
        public virtual ObservableCollection<PdfFile> PdfFiles
        {
            get { return pdfFiles; }
            set
            {
                if (!object.Equals(pdfFiles, value))
                {
                    pdfFiles = value;
                    OnPropertyChanged(() => PdfFiles);
                }
            }
        }

        private PdfFile selectedFile = null;
        public virtual PdfFile SelectedFile
        {
            get { return selectedFile; }
            set
            {
                if (selectedFile != value)
                {
                    selectedFile = value;
                    OnPropertyChanged(() => SelectedFile);
                }
            }
        }

        private string destinationFolder = string.Empty;
        public virtual string DestinationFolder
        {
            get { return destinationFolder; }
            set
            {
                if (destinationFolder != value)
                {
                    destinationFolder = value;
                    OnPropertyChanged(() => DestinationFolder);
                }
            }
        }

        private string baseFileName = string.Empty;
        public virtual string BaseFileName
        {
            get { return baseFileName; }
            set
            {
                if (baseFileName != value)
                {
                    baseFileName = value;
                    OnPropertyChanged(() => BaseFileName);
                }
            }
        }

        private bool overwriteFile = false;
        public bool OverwriteFile
        {
            get { return overwriteFile; }
            set
            {
                if (overwriteFile != value)
                {
                    overwriteFile = value;
                    OnPropertyChanged(() => OverwriteFile);
                }
            }
        }

        #endregion

        #region [ INotifyDataErrorInfo members ]

        private ConcurrentDictionary<string, ICollection<ValidationResult>> propertyErrors =
            new ConcurrentDictionary<string, ICollection<ValidationResult>>();

        protected void ClearViewErrors()
        {
            foreach (var propertyName in propertyErrors.Keys)
            {
                ClearPropertyErrors(propertyName);
            }
        }

        protected void ClearPropertyErrors(string propertyName)
        {
            if (propertyErrors.ContainsKey(propertyName))
            {
                ICollection<ValidationResult> existingErrors = null;
                propertyErrors.TryRemove(propertyName, out existingErrors);
                NotifyErrorChanged(propertyName);
            }
        }

        protected void SetErrors(string propertyName, ICollection<ValidationResult> errors)
        {
            if (propertyErrors.ContainsKey(propertyName))
            {
                ICollection<ValidationResult> existingErrors = null;
                propertyErrors.TryRemove(propertyName, out existingErrors);
            }
            propertyErrors.TryAdd(propertyName, errors);
            NotifyErrorChanged(propertyName);
        }

        public bool HasErrors
        {
            get { return propertyErrors.Count > 0; }
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) //Retrieve errors for entire object
                return propertyErrors.Values;
            else if (propertyErrors.ContainsKey(propertyName) &&
                    (propertyErrors[propertyName] != null) &&
                    propertyErrors[propertyName].Count > 0)
                return propertyErrors.GetOrAdd(propertyName, (key) => null);
            else
                return null;
        }

        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        private void NotifyErrorChanged(string propertyName)
        {
            if (ErrorsChanged != null)
                ErrorsChanged(this, new DataErrorsChangedEventArgs(propertyName));
        }

        #endregion
    }
}
