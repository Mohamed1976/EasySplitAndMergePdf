using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySplitAndMergePdf
{
    #region [ Enum defines ]

    public enum DocSplitMethod
    {
        None,
        Interval,
        Range,
    }

    public enum SaveOptions
    {
        None,
        UseSourceFolder,
        UseCustomFolder
    }

    #endregion
}
