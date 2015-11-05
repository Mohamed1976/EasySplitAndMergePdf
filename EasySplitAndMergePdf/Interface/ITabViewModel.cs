using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySplitAndMergePdf.Interface
{
    public interface ITabViewModel
    {
        /// <summary>
        /// Gets or sets the header of the TabItem.
        /// </summary>
        string Header { get; set; }
    }
}
