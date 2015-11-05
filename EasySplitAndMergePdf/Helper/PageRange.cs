using System;

namespace EasySplitAndMergePdf.Helper
{
    public class PageRange
    {
        #region [ Constructors ]

        public PageRange(int pageFrom, int pageTo)
        {
            PageFrom = pageFrom;
            PageTo = pageTo;
        }

        public PageRange(int[] pages)
        {
            Pages = pages;
        }

        #endregion

        #region [ Properties ]

        private int pageFrom = 0;
        public int PageFrom
        {
            get { return pageFrom; }
            set { pageFrom = value; }
        }

        private int pageTo = 0;
        public int PageTo
        {
            get { return pageTo; }
            set { pageTo = value; }
        }

        private int[] pages = null;
        public int[] Pages
        {
            get { return pages; }
            set { pages = value; }
        }

        public int PageCount
        {
            get
            {
                if (Pages != null)
                {
                    return Pages.Length;
                }
                else if (PageFrom != 0 && PageTo != 0)
                {
                    return Math.Abs(PageTo - PageFrom) + 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        #endregion
    }

}
