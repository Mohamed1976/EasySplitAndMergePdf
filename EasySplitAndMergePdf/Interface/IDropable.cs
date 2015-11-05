using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasySplitAndMergePdf.Interface
{
    public interface IDropable
    {
        /// <summary>
        /// Drop data into the collection.
        /// </summary>
        /// <param name="data">The data to be dropped</param>
        /// <param name="index">optional: The index location to insert the data</param>
        void Drop(object data, int index = -1);
    }
}
