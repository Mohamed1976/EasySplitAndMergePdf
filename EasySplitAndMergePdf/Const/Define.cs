using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySplitAndMergePdf
{
    public sealed class Define
    {
        #region [ Defines ]

        public static int DataGridMovementUp = -1;
        public static int DataGridMovementNone = 0;
        public static int DataGridMovementDown = 1;

        public static string defaultSplitBaseFileName = "SplitFile";
        public static string defaultMergeBaseFileName = "MergeFile";
        public static bool defaultOverwriteFile = true;
        public static SaveOptions defaultSaveType = SaveOptions.UseSourceFolder;
        public static DocSplitMethod defaultSplitMethod = DocSplitMethod.Interval;

        public static readonly int Success = 0;
        public static readonly int Failure = 1;
        public static readonly int PdfException = 2;
        public static readonly int BadPdfFormatException = 3;
        public static readonly int PdfAConformanceException = 4;
        public static readonly int PdfIsoConformanceException = 5;
        public static readonly int BadPasswordException = 6;
        public static readonly int IllegalPdfSyntaxException = 7;
        public static readonly int InvalidImageException = 8;
        public static readonly int InvalidPdfException = 9;
        public static readonly int FileAlreadyExistsAtLocation = 10;
        public static readonly int FolderIsNotSpecified = 11;
        public static readonly int FolderHasInvalidPathChars = 12;
        public static readonly int FolderDoesNotExist = 13;
        public static readonly int FileNameIsNotSpecified = 14;
        public static readonly int FileNameHasInvalidPathChars = 15;
        public static readonly int PageRangeIsNullOrEmpty = 16;
        public static readonly int PageRangeSyntaxError = 17;
        public static readonly int SpecifiedPageRangeNotValid = 18;
        public static readonly int SpecifiedPageIntervalNotValid = 19;
        public static readonly int RangeAndIntervalAreNullOrEmpty = 20;

        #endregion
    }
}
