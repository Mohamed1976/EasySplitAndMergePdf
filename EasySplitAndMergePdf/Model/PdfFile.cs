using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text.pdf;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EasySplitAndMergePdf.Model
{
    public class PdfFile : INotifyPropertyChanged, IDisposable
    {
        #region [ Constructors ]

        static PdfFile()
        {
            PdfReader.unethicalreading = true;
        }

        public PdfFile(string path)
        {
            Info = new FileInfo(path);
        }

        #endregion

        #region [ Public methods ]

        public int Open(string ownerPassword = null)
        {
            int result = Define.Failure;
            PdfReader pdfReader = null;

            try
            {
                if (string.IsNullOrEmpty(ownerPassword))
                    pdfReader = new PdfReader(fileInfo.FullName);
                else
                    pdfReader = new PdfReader(fileInfo.FullName, Encoding.ASCII.GetBytes(ownerPassword));

                result = Define.Success;
            }
            catch (iTextSharp.text.pdf.BadPdfFormatException ex)
            {
                result = Define.BadPdfFormatException;
                ErrorMsg = ex.Message;
            }
            catch (iTextSharp.text.pdf.PdfException ex)
            {
                result = Define.PdfException;
                ErrorMsg = ex.Message;
            }
            catch (iTextSharp.text.pdf.PdfIsoConformanceException ex)
            {
                result = Define.PdfIsoConformanceException;
                ErrorMsg = ex.Message;
            }
            catch (iTextSharp.text.exceptions.BadPasswordException ex)
            {
                result = Define.BadPasswordException;
                ErrorMsg = ex.Message;
            }
            catch (iTextSharp.text.exceptions.IllegalPdfSyntaxException ex)
            {
                result = Define.IllegalPdfSyntaxException;
                ErrorMsg = ex.Message;
            }
            catch (iTextSharp.text.exceptions.InvalidImageException ex)
            {
                result = Define.InvalidImageException;
                ErrorMsg = ex.Message;
            }
            catch (iTextSharp.text.exceptions.InvalidPdfException ex)
            {
                result = Define.InvalidPdfException;
                ErrorMsg = ex.Message;
            }
            catch (Exception ex)
            {
                result = Define.Failure;
                ErrorMsg = ex.Message;
            }

            if (result == Define.Success)
            {
                IsLocked = false;
            }
            else
            {
                IsLocked = true;
                if (pdfReader != null)
                {
                    pdfReader.Close();
                    pdfReader.Dispose();
                    pdfReader = null;
                }
            }
            Reader = pdfReader;
            return result;
        }

        public void Close()
        {
            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
                reader = null;
            }
        }

        public int GetProperties(out Dictionary<string, string> fileProperties,
            out Dictionary<string, string> info,
            out Dictionary<string, string> security)
        {
            int result = Define.Failure;
            Dictionary<string, string> properties = null;
            Dictionary<string, string> fileSecurity = null;

            try
            {
                properties = new Dictionary<string, string>();
                fileSecurity = new Dictionary<string, string>();
                properties.Add("Name", Info.Name);
                properties.Add("Pdf version", Reader.PdfVersion.ToString());
                properties.Add("Number of pages", Reader.NumberOfPages.ToString());
                properties.Add("DirectoryName", Info.DirectoryName);
                properties.Add("Length", Info.Length.ToString() + "(bytes)");
                properties.Add("IsReadOnly", Info.IsReadOnly.ToString());
                properties.Add("CreationTime", Info.CreationTime.ToString());
                properties.Add("LastAccessTime", Info.LastAccessTime.ToString());
                properties.Add("LastWriteTime", Info.LastWriteTime.ToString());

                fileSecurity.Add("Is encrypted", Reader.IsEncrypted().ToString());
                fileSecurity.Add("Is 128Key", Reader.Is128Key().ToString());
                fileSecurity.Add("Full permissions", Reader.IsOpenedWithFullPermissions.ToString());
                string sVal = PdfEncryptor.IsModifyAnnotationsAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Modify annotations", sVal);
                sVal = PdfEncryptor.IsModifyContentsAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Modify content", sVal);
                sVal = PdfEncryptor.IsPrintingAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Printing", sVal);
                sVal = PdfEncryptor.IsScreenReadersAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Screen readers", sVal);
                sVal = PdfEncryptor.IsDegradedPrintingAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Degraded printing", sVal);
                sVal = PdfEncryptor.IsFillInAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Fill In", sVal);
                sVal = PdfEncryptor.IsCopyAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Copy", sVal);
                sVal = PdfEncryptor.IsAssemblyAllowed((int)Reader.Permissions) ? "Allowed" : "Not allowed";
                fileSecurity.Add("Assembly", sVal);

                result = Define.Success;
            }
            catch (Exception ex)
            {
                ErrorMsg = ex.Message;
                result = Define.Failure;
            }

            if (result == Define.Success)
            {
                info = Reader.Info;
            }
            else
            {
                if (properties != null)
                {
                    properties.Clear();
                    properties = null;
                }

                if (fileSecurity != null)
                {
                    fileSecurity.Clear();
                    fileSecurity = null;
                }

                info = null;
            }

            fileProperties = properties;
            security = fileSecurity;
            return result;
        }

        #endregion

        #region [ Properties ]

        private string errorMsg = string.Empty;
        public string ErrorMsg
        {
            get { return errorMsg; }
            private set
            {
                if (errorMsg != value)
                {
                    errorMsg = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private FileInfo fileInfo = null;
        public FileInfo Info
        {
            get { return fileInfo; }
            private set
            {
                if (!object.Equals(fileInfo, value))
                {
                    fileInfo = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private PdfReader reader = null;
        public PdfReader Reader
        {
            get { return reader; }
            private set
            {
                if (!object.Equals(reader, value))
                {
                    reader = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool isLocked = true;
        public bool IsLocked
        {
            get { return isLocked; }
            private set
            {
                if (isLocked != value)
                {
                    isLocked = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion

        #region [ INotifyPropertyChanged ]

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region [ IDisposable ]

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~PdfFile()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                    reader = null;
                }
            }
        }

        #endregion
    }
    }
