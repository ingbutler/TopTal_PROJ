using System;
using System.Collections.Generic;

namespace DBRuns.Models
{

    public class ItemList<T>
    {
        public int ItemsPerPage { get; set; }
        public int PageNumber { get; set; }
        public int QueriedItemsCount { get; set; }
        public int PageCount { get; set; }

        public List<T> items;
    }

}
