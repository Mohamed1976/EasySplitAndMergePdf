using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.ComponentModel;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Interface;
using EasySplitAndMergePdf.Command;
using EasySplitAndMergePdf.Helper;
using EasySplitAndMergePdf.Model;
using EasySplitAndMergePdf.View;

namespace EasySplitAndMergePdf.ViewModel
{
    public class SplitPdfViewModel : ViewModelBase, ITabViewModel, IDropable
    {
        #region [ Defines ]

        /// <summary>
        /// Get pdf file on a separate thread.
        /// </summary>
        private BackgroundWorker fileBackgroundWorker = null;

        /// <summary>
        /// Split pdf file on a separate thread.
        /// </summary>
        private BackgroundWorker splitBackgroundWorker = null;

        #endregion

        #region[ Constructors ]

        public SplitPdfViewModel(string header)
        {
            Header = header;
            InitializeProperties();
            InitializeCommands();
            InitializeBackgroundWorker();
        }

        #endregion

        #region [ Commands]

        private void InitializeCommands()
        {
            AddFileCmd = new RelayCommand(OnAddFileCmdExecute, OnAddFileCmdCanExecute);
            RemoveFileCmd = new RelayCommand(OnRemoveFileCmdExecute, OnRemoveFileCmdCanExecute);
            SelectFolderCmd = new RelayCommand(OnSelectFolderCmdExecute);
            SplitPdfCmd = new RelayCommand(OnSplitPdfCmdExecute, OnSplitPdfCmdCanExecute);
            CancelSplitPdfCmd = new RelayCommand(OnCancelSplitPdfCmdExecute, CancelSplitPdfCmdCanExecute);
            UnlockCmd = new RelayCommand(OnUnlockCmdExecute, OnUnlockCmdCanExecute);
            ShowFilePropertiesCmd = new RelayCommand(OnShowFilePropertiesCmdExecute, OnShowFilePropertiesCmdCanExecute);
            OpenFileCmd = new RelayCommand(OnOpenFileCmdExecute, OnOpenFileCmdCanExecute);
        }

        public RelayCommand AddFileCmd { get; private set; }
        public RelayCommand RemoveFileCmd { get; private set; }
        public RelayCommand SelectFolderCmd { get; private set; }
        public RelayCommand SplitPdfCmd { get; private set; }
        public RelayCommand CancelSplitPdfCmd { get; private set; }
        public RelayCommand UnlockCmd { get; private set; }
        public RelayCommand ShowFilePropertiesCmd { get; private set; }
        public RelayCommand OpenFileCmd { get; private set; }

        private void OnAddFileCmdExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "pdf files (*.pdf)|*.pdf";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProgressBarIsIndeterminate = true;
                ProgressStatus = "Loading PDF file: " + openFileDialog.FileName;
                IsBusy = true;
                fileBackgroundWorker.RunWorkerAsync(openFileDialog.FileName);
            }
        }

        private bool OnAddFileCmdCanExecute()
        {
            return !IsBusy && SelectedFile == null;
        }

        private bool OnRemoveFileCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null;
        }

        private void OnRemoveFileCmdExecute()
        {
            SelectedFile.Close();
            App.Current.Dispatcher.Invoke(delegate () { PdfFiles.RemoveAt(0); });
            SelectedFile = null;

        }

        private void OnSelectFolderCmdExecute()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DestinationFolder = folderBrowserDialog.SelectedPath;
            }
        }

        private void OnSplitPdfCmdExecute()
        {
            string destinationPath = string.Empty;
            PageRangeParser pageRangeParser = null;

            IsBusy = true;
            bool isValid = ViewIsValid(out pageRangeParser);
            if (isValid)
            {
                if (SaveType == SaveOptions.UseSourceFolder)
                {
                    destinationPath = string.Format("{0}\\{1}", 
                        PdfFiles.First().Info.DirectoryName, BaseFileName);
                }
                else if (SaveType == SaveOptions.UseCustomFolder)
                {
                    destinationPath = string.Format("{0}\\{1}",
                        DestinationFolder, BaseFileName);
                }

                splitBackgroundWorker.RunWorkerAsync(new object[] { PdfFiles.First(),
                    pageRangeParser, destinationPath, OverwriteFile });
            }
            else
            {
                IsBusy = false;
            }
        }

        private bool OnSplitPdfCmdCanExecute()
        {
            return !IsBusy;
        }

        private void OnUnlockCmdExecute()
        {
            PdfLoginView pdfLoginView = new PdfLoginView();
            pdfLoginView.DataContext = new PdfLoginViewModel(pdfLoginView, PdfFiles.First());
            pdfLoginView.Owner = App.Current.MainWindow;
            bool? dialogResult = pdfLoginView.ShowDialog();
            if (dialogResult.HasValue && dialogResult.Value)
            {
                ProgressStatus = "PDF file successfully unlocked.";
            }
            else
            {
                ProgressStatus = "Failed to unlock PDF file.";
            }
        }

        private bool OnUnlockCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null && SelectedFile.IsLocked;
        }

        private bool OnShowFilePropertiesCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null && !SelectedFile.IsLocked;
        }

        private void OnShowFilePropertiesCmdExecute()
        {
            Dictionary<string, string> properties = null;
            Dictionary<string, string> security = null;
            Dictionary<string, string> info = null;

            if (SelectedFile.GetProperties(out properties, out info, out security) == Define.Success)
            {
                FilePropertiesViewModel filePropertiesViewModel =
                    new FilePropertiesViewModel(SelectedFile.Info.FullName, properties, info, security);
                FilePropertiesView propertiesView = new FilePropertiesView();
                propertiesView.DataContext = filePropertiesViewModel;
                propertiesView.Owner = App.Current.MainWindow;
                propertiesView.Show();
            }
        }

        private bool OnOpenFileCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null;
        }

        private void OnOpenFileCmdExecute()
        {
            WebBrowserView webBrowserView = new WebBrowserView();
            WebBrowserViewModel webBrowserViewModel = new WebBrowserViewModel(PdfFiles.First().Info.FullName);
            webBrowserView.DataContext = webBrowserViewModel;
            webBrowserView.Owner = App.Current.MainWindow;
            webBrowserView.Show();
        }

        private bool CancelSplitPdfCmdCanExecute()
        {
            return splitBackgroundWorker.IsBusy;
        }

        private void OnCancelSplitPdfCmdExecute()
        {
            splitBackgroundWorker.CancelAsync();
        }

        #endregion

        #region [ BackgroundWorker ]

        private void InitializeBackgroundWorker()
        {
            splitBackgroundWorker = new BackgroundWorker();
            fileBackgroundWorker = new BackgroundWorker();

            fileBackgroundWorker.DoWork += new DoWorkEventHandler(FileWorkerDoWork);
            fileBackgroundWorker.RunWorkerCompleted += 
                new RunWorkerCompletedEventHandler(FileWorkerRunWorkerCompleted);

            splitBackgroundWorker.WorkerReportsProgress = true;
            splitBackgroundWorker.WorkerSupportsCancellation = true;
            splitBackgroundWorker.DoWork += 
                new DoWorkEventHandler(SplitWorkerDoWork);
            splitBackgroundWorker.ProgressChanged += 
                new ProgressChangedEventHandler(SplitWorkerProgressChanged);
            splitBackgroundWorker.RunWorkerCompleted += 
                new RunWorkerCompletedEventHandler(SplitWorkerRunWorkerCompleted);
        }

        private void FileWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = e.Argument.ToString();

            PdfFile pdfFile = new PdfFile(fileName);
            int status = pdfFile.Open();

            if (status == Define.Success ||
                status == Define.BadPasswordException)
            {
                App.Current.Dispatcher.Invoke(delegate ()
                {
                    PdfFiles.Add(pdfFile);
                    SelectedFile = PdfFiles.First();
                });
            }
            else
            {
                e.Result = pdfFile.ErrorMsg;
            }
        }

        private void FileWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = string.Empty;

            if (e.Error != null)
            {
                result = "An error was thrown while loading PDF file: " + e.Error.ToString();
            }
            else if (e.Result != null && !string.IsNullOrEmpty(e.Result.ToString()))
            {
                result = e.Result.ToString();
            }

            ProgressStatus = result;
            ProgressBarIsIndeterminate = false;
            IsBusy = false;
        }

        private void SplitWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            PdfFile pdfFile = (PdfFile)args[0];
            PageRangeParser pageRangeParser = (PageRangeParser)args[1];
            string destinationPath = (string)args[2];
            bool overwriteFile = (bool)args[3];
            FileStream outFileStream = null;
            Document destinationDoc = null;
            PdfCopy pdfCopy = null;
            int skippedFiles = 0;
            string exportFileName = string.Empty;
            string errorMsg = string.Empty;
            int exportFileCnt = 0, totalNumberOPages = 0, pageCnt = 0;
            EasySplitAndMergePdf.Helper.PageRange[] pageRanges = null;

            if (pageRangeParser.TryParse(out pageRanges) != Define.Success)
            {
                errorMsg = "An error occurred while parsing PDF page ranges" + pageRangeParser.ErrorMsg;
            }
            else if ((totalNumberOPages = pageRanges.Sum(range => range.PageCount)) < 1)
            {
                errorMsg = "The number of PDF pages to extract from source file is zero.";
            }
            else
            {
                pdfFile.Reader.RemoveUnusedObjects();

                while (exportFileCnt < pageRanges.Length && !splitBackgroundWorker.CancellationPending)
                {
                    exportFileName = destinationPath + (exportFileCnt + 1).ToString("D4") + ".pdf";
                    if (FileHelpers.FileIsAvailable(exportFileName, overwriteFile, out outFileStream, out errorMsg) == Define.Success)
                    {
                        destinationDoc = new Document();
                        pdfCopy = new PdfCopy(destinationDoc, outFileStream);
                        destinationDoc.Open();

                        splitBackgroundWorker.ReportProgress(pageCnt * 100 / totalNumberOPages,
                            string.Format("Creating and processing PDF file: {0}", exportFileName));

                        if (pageRanges[exportFileCnt].Pages != null)
                        {
                            int pageArrayIndex = 0;
                            while (pageArrayIndex < pageRanges[exportFileCnt].Pages.Length &&
                                !splitBackgroundWorker.CancellationPending)
                            {
                                destinationDoc.SetPageSize(pdfFile.Reader.GetPageSizeWithRotation(pageRanges[exportFileCnt].Pages[pageArrayIndex]));
                                destinationDoc.NewPage();
                                pdfCopy.AddPage(pdfCopy.GetImportedPage(pdfFile.Reader, pageRanges[exportFileCnt].Pages[pageArrayIndex]));
                                splitBackgroundWorker.ReportProgress(++pageCnt * 100 / totalNumberOPages);
                                pageArrayIndex++;
                            }
                        }
                        else if (pageRanges[exportFileCnt].PageFrom <= pageRanges[exportFileCnt].PageTo)
                        {
                            int pageNumber = pageRanges[exportFileCnt].PageFrom;
                            while (pageNumber <= pageRanges[exportFileCnt].PageTo &&
                                !splitBackgroundWorker.CancellationPending)
                            {
                                destinationDoc.SetPageSize(pdfFile.Reader.GetPageSizeWithRotation(pageNumber));
                                destinationDoc.NewPage();
                                pdfCopy.AddPage(pdfCopy.GetImportedPage(pdfFile.Reader, pageNumber));
                                splitBackgroundWorker.ReportProgress(++pageCnt * 100 / totalNumberOPages);
                                pageNumber++;
                            }
                        }
                        else if (pageRanges[exportFileCnt].PageFrom > pageRanges[exportFileCnt].PageTo)
                        {
                            int pageNumber = pageRanges[exportFileCnt].PageFrom;
                            while (pageNumber >= pageRanges[exportFileCnt].PageTo &&
                                !splitBackgroundWorker.CancellationPending)
                            {
                                destinationDoc.SetPageSize(pdfFile.Reader.GetPageSizeWithRotation(pageNumber));
                                destinationDoc.NewPage();
                                pdfCopy.AddPage(pdfCopy.GetImportedPage(pdfFile.Reader, pageNumber));
                                splitBackgroundWorker.ReportProgress(++pageCnt * 100 / totalNumberOPages);
                                pageNumber--;
                            }
                        }

                        //Exception on document.Close() when doc is empty
                        //if (pages.Count == 0) { throw new IOException("The document has no pages.") }; 
                        //When canceling pages.Count could be zero therefore skip cleanup. 
                        if (destinationDoc != null &&
                            !splitBackgroundWorker.CancellationPending)
                        {
                            destinationDoc.Close();
                            destinationDoc.Dispose();
                            destinationDoc = null;
                        }

                        if (pdfCopy != null &&
                            !splitBackgroundWorker.CancellationPending)
                        {
                            pdfCopy.Close();
                            pdfCopy.Dispose();
                            pdfCopy = null;
                        }

                        if (outFileStream != null)
                        {
                            outFileStream.Close();
                            outFileStream.Dispose();
                            outFileStream = null;
                        }
                    }
                    else
                    {
                        skippedFiles++;
                        Debug.WriteLine(string.Format("File: {0}, error: {1}", exportFileName, errorMsg));
                    }
                    exportFileCnt++;
                }
            }

            if (string.IsNullOrEmpty(errorMsg) &&
                exportFileCnt == pageRanges.Length &&
                skippedFiles == 0)
            {
                errorMsg = string.Format("Successfully created {0} PDF export files.", pageRanges.Length);
            }
            else if (skippedFiles > 0)
            {
                errorMsg = string.Format("Created {0} PDF export files, skipped {1} PDF files.",
                    pageRanges.Length - skippedFiles, skippedFiles);
            }

            if (splitBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }

            e.Result = errorMsg;
        }

        private void SplitWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
            if (e.UserState != null && !string.IsNullOrEmpty(e.UserState.ToString()))
            {
                ProgressStatus = e.UserState.ToString();
            }
        }

        private void SplitWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = string.Empty;

            if (e.Error != null)
            {
                result = "An error occurred while processing PDF file" + e.Error.ToString();
            }
            else if (e.Cancelled)
            {
                result = "PDF file processing was canceled by user.";
            }
            else if (e.Result != null && !string.IsNullOrEmpty(e.Result.ToString()))
            {
                result = e.Result.ToString();
            }

            ProgressBarValue = 0;
            ProgressStatus = result;
            IsBusy = false;
        }

        #endregion

        #region [ InitializeProperties ]

        private void InitializeProperties()
        {
            PdfFiles = new ObservableCollection<PdfFile>();
            DestinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            BaseFileName = Define.defaultSplitBaseFileName;
            OverwriteFile = Define.defaultOverwriteFile;
            SplitMethod = Define.defaultSplitMethod;
            SaveType = Define.defaultSaveType;
        }

        #endregion

        #region [ Properties ]

        string pageInterval = string.Empty;
        public string PageInterval
        {
            get { return pageInterval; }
            set { SetProperty(ref pageInterval, value); }
        }

        string pageRange = string.Empty;
        public string PageRange
        {
            get { return pageRange; }
            set { SetProperty(ref pageRange, value); }
        }

        private DocSplitMethod splitMethod = DocSplitMethod.None;
        public DocSplitMethod SplitMethod
        {
            get { return splitMethod; }
            set { SetProperty(ref splitMethod, value); }
        }

        private SaveOptions saveType = SaveOptions.None;
        public SaveOptions SaveType
        {
            get { return saveType; }
            set { SetProperty(ref saveType, value); }
        }

        #endregion

        #region [ View Validation ]

        private bool ViewIsValid(out PageRangeParser pageRangeParser)
        {
            bool isValid = false;
            string errorMsg = string.Empty;
            pageRangeParser = null;

            ClearViewErrors();
            if (PdfFiles.Count == 0)
            {
                ProgressStatus = "Please select a PDF file to split using the + button or [ Ins ].";
            }
            else if (PdfFiles.First().IsLocked)
            {
                ProgressStatus = "Please unlock the PDF file by clicking the lock button in the list or [ Ctrl+U ].";
            }
            else
            {
                if (FileHelpers.FileNameIsValid(BaseFileName, out errorMsg) != Define.Success)
                {
                    SetErrors("BaseFileName", new List<ValidationResult>() { new ValidationResult(false, errorMsg) });
                }

                if (SaveType == SaveOptions.UseCustomFolder &&
                    FileHelpers.FolderIsValid(DestinationFolder, out errorMsg) != Define.Success)
                {
                    SetErrors("DestinationFolder", new List<ValidationResult>() { new ValidationResult(false, errorMsg) });
                }

                errorMsg = string.Empty;
                if (SplitMethod == DocSplitMethod.Interval)
                {
                    if (string.IsNullOrEmpty(PageInterval))
                    {
                        errorMsg = "Split page interval is required.";
                    }
                    else
                    {
                        pageRangeParser = new PageRangeParser(PdfFiles.First().Reader.NumberOfPages, int.Parse(PageInterval));
                        if (pageRangeParser.Validate() != Define.Success)
                        {
                            errorMsg = pageRangeParser.ErrorMsg;
                        }
                    }

                    if (errorMsg != string.Empty)
                    {
                        SetErrors("PageInterval", new List<ValidationResult>() { new ValidationResult(false, errorMsg) });
                    }
                }
                else if (SplitMethod == DocSplitMethod.Range)
                {
                    if (string.IsNullOrEmpty(PageRange))
                    {
                        errorMsg = "Split page range is required.";
                    }
                    else
                    {
                        pageRangeParser = new PageRangeParser(PdfFiles.First().Reader.NumberOfPages, PageRange);
                        if (pageRangeParser.Validate() != Define.Success)
                        {
                            errorMsg = pageRangeParser.ErrorMsg;
                        }
                    }

                    if (errorMsg != string.Empty)
                    {
                        SetErrors("PageRange", new List<ValidationResult>() { new ValidationResult(false, errorMsg) });
                    }
                }

                isValid = !HasErrors;
            }
            return isValid;
        }

        #endregion

        #region [ IDropable members ]

        public void Drop(object data, int index = -1)
        {
            if (!IsBusy)
            {
                string[] fileNames = (string[])data;
                ProgressBarIsIndeterminate = true;
                ProgressStatus = "Loading dropped PDF file: " + fileNames[0];
                IsBusy = true;
                fileBackgroundWorker.RunWorkerAsync(fileNames[0]);
            }
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
