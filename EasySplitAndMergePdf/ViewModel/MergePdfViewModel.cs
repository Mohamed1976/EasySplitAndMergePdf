using System;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Linq;
using EasySplitAndMergePdf.Base;
using EasySplitAndMergePdf.Interface;
using EasySplitAndMergePdf.Command;
using EasySplitAndMergePdf.Model;
using EasySplitAndMergePdf.Helper;
using EasySplitAndMergePdf.View;

namespace EasySplitAndMergePdf.ViewModel
{
    public class MergePdfViewModel : ViewModelBase, ITabViewModel, IDropable
    {
        #region [ Defines ]

        /// <summary>
        /// Get pdf file on a separate thread.
        /// </summary>
        private BackgroundWorker fileBackgroundWorker = null;

        /// <summary>
        /// Split pdf file on a separate thread.
        /// </summary>
        private BackgroundWorker mergeBackgroundWorker = null;

        #endregion

        #region[ Constructors ]

        public MergePdfViewModel(string header)
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
            ShowFilePropertiesCmd = new RelayCommand(OnShowFilePropertiesCmdExecute, OnShowFilePropertiesCmdCanExecute);
            MoveUpCmd = new RelayCommand(OnMoveUpCmdExecute, OnMoveUpCmdCanExecute);
            MoveDownCmd = new RelayCommand(OnMoveDownCmdExecute, OnMoveDownCmdCanExecute);
            SelectFolderCmd = new RelayCommand(OnSelectFolderCmdExecute, OnSelectFolderCmdCanExecute);
            OpenFileCmd = new RelayCommand(OnOpenFileCmdExecute, OnOpenFileCmdCanExecute);
            MergeCmd = new RelayCommand(OnMergeCmdExecute, OnMergeCmdCanExecute);
            CancelMergeCmd = new RelayCommand(OnCancelMergeCmdExecute, OnCancelMergeCmdCanExecute);
            UnlockCmd = new RelayCommand(OnUnlockCmdExecute, OnUnlockCmdCanExecute);
            ScrollUpCmd = new RelayCommand(OnScrollUpCmdExecute, OnScrollUpCmdCanExecute);
            ScrollDownCmd = new RelayCommand(OnScrollDownCmdExecute, OnScrollDownCmdCanExecute);
        }

        public RelayCommand AddFileCmd { get; private set; }
        public RelayCommand RemoveFileCmd { get; private set; }
        public RelayCommand ShowFilePropertiesCmd { get; private set; }
        public RelayCommand MoveUpCmd { get; private set; }
        public RelayCommand MoveDownCmd { get; private set; }
        public RelayCommand SelectFolderCmd { get; private set; }
        public RelayCommand OpenFileCmd { get; private set; }
        public RelayCommand MergeCmd { get; private set; }
        public RelayCommand CancelMergeCmd { get; private set; }
        public RelayCommand UnlockCmd { get; private set; }
        public RelayCommand ScrollUpCmd { get; private set; }
        public RelayCommand ScrollDownCmd { get; private set; }
        public RelayCommand OverwriteCmd { get; private set; }
        public RelayCommand SelectDestinationCmd { get; private set; }

        private bool OnAddFileCmdCanExecute()
        {
            return !IsBusy;
        }

        private void OnAddFileCmdExecute()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "pdf files (*.pdf)|*.pdf";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                ProgressBarIsIndeterminate = true;
                ProgressStatus = "Loading selected PDF file(s).";
                IsBusy = true;
                fileBackgroundWorker.RunWorkerAsync(new object[] { openFileDialog.FileNames });
            }
        }

        private bool OnRemoveFileCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null;
        }

        private void OnRemoveFileCmdExecute()
        {
            SelectedFile.Close();
            int index = PdfFiles.IndexOf(SelectedFile);
            App.Current.Dispatcher.Invoke(delegate { PdfFiles.RemoveAt(index); });
            if (PdfFiles.Count > index)
            {
                SelectedFile = PdfFiles[index];
            }
            else if (PdfFiles.Any())
            {
                SelectedFile = PdfFiles[index - 1];
            }
            else
            {
                SelectedFile = null;
            }
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
                                        new FilePropertiesViewModel(SelectedFile.Info.FullName,
                                        properties, info, security);
                FilePropertiesView propertiesView = new FilePropertiesView();
                propertiesView.DataContext = filePropertiesViewModel;
                propertiesView.Owner = App.Current.MainWindow;
                propertiesView.Show();
            }
        }

        private bool OnMoveUpCmdCanExecute()
        {
            if (!IsBusy &&
                SelectedFile != null &&
                PdfFiles.IndexOf(SelectedFile) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnMoveUpCmdExecute()
        {
            MoveUpAndDown(Define.DataGridMovementUp);
        }

        private bool OnMoveDownCmdCanExecute()
        {
            if (!IsBusy &&
                SelectedFile != null &&
                PdfFiles.IndexOf(SelectedFile) < PdfFiles.Count - 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void OnMoveDownCmdExecute()
        {        
            MoveUpAndDown(Define.DataGridMovementDown);
        }

        private void MoveUpAndDown(int direction)
        {
            IsBusy = true;
            int index = PdfFiles.IndexOf(SelectedFile);
            App.Current.Dispatcher.Invoke(delegate
            {
                PdfFiles.Move(index, index + direction);
            });

            //Needed to trigger Selection Changed event In datagrid
            SelectedFile = null;
            SelectedFile = PdfFiles[index + direction];
            IsBusy = false;
        }

        private bool OnSelectFolderCmdCanExecute()
        {
            return !IsBusy;
        }

        private void OnSelectFolderCmdExecute()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                DestinationFolder = folderBrowserDialog.SelectedPath;
        }

        private bool OnOpenFileCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null;
        }

        private void OnOpenFileCmdExecute()
        {
            WebBrowserView webBrowserView = new WebBrowserView();
            WebBrowserViewModel webBrowserViewModel = new WebBrowserViewModel(SelectedFile.Info.FullName);
            webBrowserView.DataContext = webBrowserViewModel;
            webBrowserView.Owner = App.Current.MainWindow;
            webBrowserView.Show();
        }

        private bool OnMergeCmdCanExecute()
        {
            return !IsBusy;
        }

        private void OnMergeCmdExecute()
        {
            IsBusy = true;
            bool isValid = ViewIsValid();

            if (isValid)
            {
                string destinationPath = string.Format("{0}\\{1}.pdf", 
                    DestinationFolder, BaseFileName); 
                mergeBackgroundWorker.RunWorkerAsync(new object[] { PdfFiles,
                    destinationPath, OverwriteFile });
            }
            else
            {
                IsBusy = false;
            }
        }

        private bool OnCancelMergeCmdCanExecute()
        {
            return mergeBackgroundWorker.IsBusy;
        }

        private void OnCancelMergeCmdExecute()
        {
            mergeBackgroundWorker.CancelAsync();
        }

        private bool OnUnlockCmdCanExecute()
        {
            return !IsBusy && SelectedFile != null && SelectedFile.IsLocked;
        }

        private void OnUnlockCmdExecute()
        {
            PdfLoginView pdfLoginView = new PdfLoginView();
            pdfLoginView.DataContext = new PdfLoginViewModel(pdfLoginView, SelectedFile);
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

        private bool OnScrollUpCmdCanExecute()
        {
            bool canExecute = false;

            if (PdfFiles.Count > 1 &&
                SelectedFile != null &&
                PdfFiles.IndexOf(SelectedFile) > 0)
            {
                canExecute = true;
            }
            return canExecute;
        }

        private void OnScrollUpCmdExecute()
        {
            int index = PdfFiles.IndexOf(SelectedFile);
            SelectedFile = PdfFiles.ElementAt(--index);
        }

        private bool OnScrollDownCmdCanExecute()
        {
            bool canExecute = false;

            if (PdfFiles.Count > 1
                && SelectedFile != null &&
                PdfFiles.IndexOf(SelectedFile) < PdfFiles.Count - 1)
            {
                canExecute = true;
            }
            return canExecute;
        }

        private void OnScrollDownCmdExecute()
        {
            int index = PdfFiles.IndexOf(SelectedFile);
            SelectedFile = PdfFiles.ElementAt(++index);
        }

        #endregion

        #region [ BackgroundWorker ]

        private void InitializeBackgroundWorker()
        {
            mergeBackgroundWorker = new BackgroundWorker();
            fileBackgroundWorker = new BackgroundWorker();

            fileBackgroundWorker.WorkerReportsProgress = true;
            fileBackgroundWorker.DoWork += new DoWorkEventHandler(FileWorkerDoWork);
            fileBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(FileWorkerRunWorkerCompleted);
            fileBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(FileWorkerProgressChanged);

            mergeBackgroundWorker.WorkerReportsProgress = true;
            mergeBackgroundWorker.WorkerSupportsCancellation = true;
            mergeBackgroundWorker.DoWork += new DoWorkEventHandler(MergeWorkerDoWork);
            mergeBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(MergeWorkerProgressChanged);
            mergeBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(MergeWorkerRunWorkerCompleted);
        }

        private void FileWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            int insertIndex = -1, skippedFiles = 0;
            PdfFile pdfFile = null;

            object[] args = (object[])e.Argument;
            string[] fileNames = (string[])args[0];
            if (args.Length == 2)
            {
                insertIndex = int.Parse(args[1].ToString());
            }

            foreach (string fileName in fileNames)
            {
                fileBackgroundWorker.ReportProgress(0, string.Format("Loading PDF file: {0}", fileName));
                pdfFile = new PdfFile(fileName);
                int status = pdfFile.Open();

                if (status == Define.Success || status == Define.BadPasswordException)
                {
                    if (insertIndex == -1)
                    {
                        App.Current.Dispatcher.Invoke(delegate () { PdfFiles.Add(pdfFile); });
                    }
                    else
                    {
                        App.Current.Dispatcher.Invoke(delegate () { PdfFiles.Insert(insertIndex, pdfFile); });
                        insertIndex++;
                    }
                }
                else
                {
                    skippedFiles++;
                }
            }
            e.Result = new object[] { skippedFiles, insertIndex };
        }

        private void FileWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = string.Empty;
            SelectedFile = null;

            if (e.Error != null)
            {
                result = "An error was thrown while loading PDF file(s): " + e.Error.ToString();
                if (PdfFiles.Any())
                {
                    SelectedFile = PdfFiles.Last();
                }
            }
            else
            {
                object[] args = (object[])e.Result;
                int skippedFiles = int.Parse(args[0].ToString());
                int insertIndex = int.Parse(args[1].ToString());

                if (PdfFiles.Any())
                {
                    if(insertIndex == -1)
                    {
                        SelectedFile = PdfFiles.Last();
                    }
                    else if(insertIndex > 0 && insertIndex < PdfFiles.Count)
                    {
                        SelectedFile = PdfFiles[insertIndex -1];
                    }
                }

                if (skippedFiles > 0)
                {
                    result = string.Format("Due to PDF file errors, {0} PDF files could not be loaded.", skippedFiles);
                }
            }

            ProgressStatus = result;
            ProgressBarIsIndeterminate = false;
            IsBusy = false;
        }

        private void FileWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressStatus = e.UserState.ToString();
        }

        private void MergeWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            object[] args = (object[])e.Argument;
            ObservableCollection<PdfFile> pdfFiles = (ObservableCollection<PdfFile>)args[0];
            string destinationPath = (string)args[1];
            bool overwriteFile = (bool)args[2];
            string errorMsg = string.Empty, progressStatus = string.Empty;
            int rotation = 0, totalNrOfPages = 0, progress = 0;
            int fileIndex = 0, pageNumber = 0;
            FileStream outFileStream = null;
            Document destinationDoc = null;
            PdfImportedPage page = null;
            PdfWriter writer = null;
            PdfReader pdfReader = null;

            totalNrOfPages = pdfFiles.Sum(pdfFile => pdfFile.Reader.NumberOfPages);

            if (totalNrOfPages > 0 &&
                FileHelpers.FileIsAvailable(destinationPath,
                overwriteFile,
                out outFileStream,
                out errorMsg) == Define.Success)
            {
                destinationDoc = new Document();
                writer = PdfWriter.GetInstance(destinationDoc, outFileStream);
                destinationDoc.Open();
                PdfContentByte cb = writer.DirectContent;

                while (fileIndex < pdfFiles.Count &&
                    !mergeBackgroundWorker.CancellationPending)
                {
                    progressStatus = string.Format("Processing PDF file: {0}.", pdfFiles[fileIndex].Info.FullName);
                    mergeBackgroundWorker.ReportProgress(progress * 100 / totalNrOfPages, progressStatus);
                    pdfReader = pdfFiles[fileIndex].Reader;
                    pdfReader.RemoveUnusedObjects();
                    pageNumber = 1;
                    while (pageNumber <= pdfFiles[fileIndex].Reader.NumberOfPages &&
                        !mergeBackgroundWorker.CancellationPending)
                    {
                        destinationDoc.SetPageSize(pdfReader.GetPageSizeWithRotation(pageNumber));
                        destinationDoc.NewPage();
                        page = writer.GetImportedPage(pdfReader, pageNumber);
                        rotation = pdfReader.GetPageRotation(pageNumber);
                        if (rotation == 90)
                        {
                            cb.AddTemplate(page, 0, -1f, 1f, 0, 0, pdfReader.GetPageSizeWithRotation(pageNumber).Height);
                        }
                        else if(rotation == 180)
                        {
                            cb.AddTemplate(page, -1f, 0, 0, -1f, pdfReader.GetPageSizeWithRotation(pageNumber).Width, 
                                pdfReader.GetPageSizeWithRotation(pageNumber).Height);
                        }
                        else if(rotation == 270)
                        {
                            cb.AddTemplate(page, 0, 1f, -1f, 0, pdfReader.GetPageSizeWithRotation(pageNumber).Width, 0);
                        }
                        else
                        {
                            cb.AddTemplate(page, 1f, 0, 0, 1f, 0, 0);
                        }
                        pageNumber++;
                        mergeBackgroundWorker.ReportProgress(++progress * 100 / totalNrOfPages);
                    }
                    fileIndex++;
                }

                progressStatus = string.Format("Closing PDF file: {0}.", destinationPath);
                mergeBackgroundWorker.ReportProgress(progress * 100 / totalNrOfPages, progressStatus);
            }

            if (destinationDoc != null &&
                !mergeBackgroundWorker.CancellationPending)
            {
                destinationDoc.Close();
                destinationDoc.Dispose();
                destinationDoc = null;
            }

            if (writer != null &&
                !mergeBackgroundWorker.CancellationPending)
            {
                writer.Close();
                writer.Dispose();
                writer = null;
            }

            if (outFileStream != null)
            {
                outFileStream.Close();
                outFileStream.Dispose();
                outFileStream = null;
            }

            if (mergeBackgroundWorker.CancellationPending)
            {
                e.Cancel = true;
            }

            e.Result = errorMsg;
        }

        private void MergeWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
            if (e.UserState != null && !string.IsNullOrEmpty(e.UserState.ToString()))
            {
                ProgressStatus = e.UserState.ToString();
            }
        }

        private void MergeWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string result = string.Empty;

            if (e.Error != null)
            {
                result = "An error was thrown while merging PDF files: " + e.Error.ToString();
            }
            else if (e.Cancelled)
            {
                result = "PDF merg process was canceled by user.";
            }
            else if (e.Result != null && !string.IsNullOrEmpty(e.Result.ToString()))
            {
                result = e.Result.ToString();
            }
            else
            {
                result = "Finished merging PDF files.";
            }

            ProgressStatus = result;
            ProgressBarValue = 0;
            IsBusy = false;
        }

        #endregion

        #region [ InitializeProperties ]

        private void InitializeProperties()
        {
            PdfFiles = new ObservableCollection<PdfFile>();
            DestinationFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            BaseFileName = Define.defaultMergeBaseFileName;
            OverwriteFile = Define.defaultOverwriteFile;
        }

        #endregion

        #region [ View Validation ]

        private bool ViewIsValid()
        {
            bool isValid = false;
            string errorMsg = string.Empty;

            ClearViewErrors();
            if (PdfFiles.Count < 2)
            {
                ProgressStatus = "Please make sure you select at least two PDF files to merge using the + button or [ Ins ].";
            }
            else if (PdfFiles.Where(file => file.IsLocked == true).Any())
            {
                ProgressStatus = "Please unlock any locked PDF file by clicking the lock button in the list or [ Ctrl+U ].";
            }
            else
            {
                if (FileHelpers.FileNameIsValid(BaseFileName, out errorMsg) != Define.Success)
                {
                    SetErrors("BaseFileName", new List<ValidationResult>() { new ValidationResult(false, errorMsg) });
                }

                if (FileHelpers.FolderIsValid(DestinationFolder, out errorMsg) != Define.Success)
                {
                    SetErrors("DestinationFolder", new List<ValidationResult>() { new ValidationResult(false, errorMsg) });
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
                ProgressStatus = "Loading dropped file(s).";
                IsBusy = true;
                fileBackgroundWorker.RunWorkerAsync(new object[] { fileNames, index });
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
