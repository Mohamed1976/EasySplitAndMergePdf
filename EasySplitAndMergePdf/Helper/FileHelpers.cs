using System;
using System.IO;
using System.Linq;
using EasySplitAndMergePdf;

namespace EasySplitAndMergePdf.Helper
{
    public static class FileHelpers
    {
        #region[ Public properties ]

        public static int FileIsAvailable(string filePath,
                                            bool overwriteMode,
                                            out FileStream fileStream,
                                            out string errorMsg)
        {
            fileStream = null;
            errorMsg = string.Empty;
            int result = Define.Failure;

            try
            {
                if (!overwriteMode && File.Exists(filePath))
                {
                    result = Define.FileAlreadyExistsAtLocation;
                    errorMsg = "File already exists at location.";
                }
                else
                {
                    fileStream = new FileStream(filePath, FileMode.Create);
                    result = Define.Success;
                }
            }
            catch (Exception ex)
            {
                result = Define.Failure;
                errorMsg = ex.Message;
                if (fileStream != null)
                {
                    fileStream.Close();
                    fileStream.Dispose();
                    fileStream = null;
                }
            }
            return result;
        }

        public static int FolderIsValid(string folderPath, out string errorMsg)
        {
            int result = Define.Success;
            errorMsg = string.Empty;

            if (string.IsNullOrEmpty(folderPath))
            {
                errorMsg = "Folder is not specified.";
                result = Define.FolderIsNotSpecified;
            }
            else if (folderPath.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                string stringResult =
                    new string(System.IO.Path.GetInvalidPathChars().Where(c => !char.IsControl(c)).ToArray());
                errorMsg = string.Format("Folder path contains invalid characters: [{0}].", stringResult);
                result = Define.FolderHasInvalidPathChars;
            }
            else if (!Directory.Exists(folderPath))
            {
                errorMsg = "Specified folder does not exist.";
                result = Define.FolderDoesNotExist;
            }
            return result;
        }

        public static int FileNameIsValid(string fileName, out string errorMsg)
        {
            int result = Define.Success;
            errorMsg = string.Empty;

            if (string.IsNullOrEmpty(fileName))
            {
                errorMsg = "File name is not specified.";
                result = Define.FileNameIsNotSpecified;
            }
            else if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                string stringResult = new string(System.IO.Path.GetInvalidFileNameChars().Where(c => !char.IsControl(c)).ToArray());
                errorMsg = string.Format("File name contains invalid characters: [{0}].", stringResult);
                result = Define.FileNameHasInvalidPathChars;
            }
            return result;
        }

        #endregion
    }
}
