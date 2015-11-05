using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace EasySplitAndMergePdf.Helper
{
    public class PageRangeParser
    {
        #region [ Defines ]

        private readonly char[] delimiter = new char[] { ';' };
        private readonly char[] dash = new char[] { '-' };
        private readonly char[] comma = new char[] { ',' };
        private readonly string pageRangePattern
            = @"^((\d{1,4}-\d{1,4};)*|((\d{1,4},)*(\d{1,4};))*)*(((\d{1,4},)*(\d{1,4}))|(\d{1,4}-\d{1,4})){1};?$";

        #endregion

        #region [ Constructors ]

        public PageRangeParser(int numberOfPages, int pageInterval)
        {
            PageInterval = pageInterval;
            NumberOfPages = numberOfPages;
        }

        public PageRangeParser(int numberOfPages, string pageRanges)
        {
            PageRanges = pageRanges;
            NumberOfPages = numberOfPages;
        }

        #endregion

        #region [ Properties ]

        private int pageInterval = -1;
        public int PageInterval
        {
            get { return pageInterval; }
            set { pageInterval = value; }
        }

        private string pageRanges = string.Empty;
        public string PageRanges
        {
            get { return pageRanges; }
            set { pageRanges = value; }
        }

        private int numberOfPages = 0;
        public int NumberOfPages
        {
            get { return numberOfPages; }
            set { numberOfPages = value; }
        }

        private string errorMsg = string.Empty;
        public string ErrorMsg
        {
            get { return errorMsg; }
            set { errorMsg = value; }
        }

        #endregion

        #region [ Private methods ]

        private int ValidatePageRange()
        {
            int result = Define.Failure;

            if (string.IsNullOrEmpty(PageRanges))
            {
                result = Define.PageRangeIsNullOrEmpty;
                ErrorMsg = "Page range is required.";
            }
            else if (!Regex.IsMatch(PageRanges, pageRangePattern, RegexOptions.Singleline))
            {
                result = Define.PageRangeSyntaxError;
                ErrorMsg = "Please enter a valid page range (see example for syntax).";
            }
            else
            {
                char[] delimiters = new char[] { '-', ';', ',' };
                int[] intArray = PageRanges.Split(delimiters,
                    StringSplitOptions.RemoveEmptyEntries).Select(str => int.Parse(str)).ToArray();
                if (intArray.Min() < 1 || intArray.Max() > NumberOfPages)
                {
                    result = Define.SpecifiedPageRangeNotValid;
                    ErrorMsg = string.Format("Please enter a valid page range between 1 and {0}.", NumberOfPages);
                }
                else
                {
                    result = Define.Success;
                }
            }
            return result;
        }

        private int ValidatePageInterval()
        {
            int result = Define.Failure;

            if (PageInterval < 1 || PageInterval > NumberOfPages)
            {
                result = Define.SpecifiedPageIntervalNotValid;
                ErrorMsg = string.Format("Please enter a valid page interval between 1 and {0}.", NumberOfPages);
            }
            else
            {
                result = Define.Success;
            }
            return result;
        }

        #endregion

        #region [ Public methods ]

        public int Validate()
        {
            if (PageInterval > -1)
            {
                return ValidatePageInterval();
            }
            else if (!string.IsNullOrEmpty(PageRanges))
            {
                return ValidatePageRange();
            }
            else
            {
                ErrorMsg = string.Format("Please enter a page range or interval.");
                return Define.RangeAndIntervalAreNullOrEmpty;
            }
        }

        public int TryParse(out PageRange[] pageRanges)
        {
            int result = Define.Success;
            pageRanges = null;

            try
            {
                if (PageInterval > -1)
                {
                    int ranges = NumberOfPages / PageInterval;
                    if (numberOfPages % PageInterval != 0)
                    {
                        ranges++;
                    }
                    pageRanges = new PageRange[ranges];
                    for (int rangeCnt = 0; rangeCnt < ranges; rangeCnt++)
                    {
                        pageRanges[rangeCnt] =
                            new PageRange(rangeCnt * PageInterval + 1, (rangeCnt + 1) * PageInterval);
                    }

                    if (numberOfPages % PageInterval != 0)
                    {
                        pageRanges[ranges - 1].PageTo = NumberOfPages;
                    }
                }
                else if (!string.IsNullOrEmpty(PageRanges))
                {
                    string[] ranges = PageRanges.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    pageRanges = new PageRange[ranges.Length];

                    for (int rangeCnt = 0; rangeCnt < ranges.Length; rangeCnt++)
                    {
                        if (ranges[rangeCnt].IndexOfAny(dash) > 0)
                        {
                            int[] pages = ranges[rangeCnt].Split(dash).Select(str => int.Parse(str)).ToArray();
                            pageRanges[rangeCnt] =
                                new PageRange(pages[0], pages[1]);
                        }
                        else
                        {
                            int[] pages = ranges[rangeCnt].Split(comma).Select(str => int.Parse(str)).ToArray();
                            pageRanges[rangeCnt] = new PageRange(pages);
                        }
                    }
                }
                else
                {
                    ErrorMsg = string.Format("Please enter a page range or interval.");
                    result = Define.RangeAndIntervalAreNullOrEmpty;
                }
            }
            catch (Exception ex)
            {
                result = Define.Failure;
                ErrorMsg = "An error occurred while processing files" + ex.Message;
            }
            return result;
        }

        #endregion
    }
}
