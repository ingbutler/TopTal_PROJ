using System;
using System.Collections.Generic;

namespace DBRuns.Models
{

    public class UserList
    {
        public int ItemsPerPage { get; set; }
        public int PageNumber { get; set; }
        public int QueriedItemsCount { get; set; }
        public int PageCount { get; set; }

        public List<User> users;
    }

}
