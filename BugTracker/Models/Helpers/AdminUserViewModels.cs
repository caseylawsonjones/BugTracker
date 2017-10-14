using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.Helpers {
    public class AdminUserViewModels {

        public ApplicationUser User { get; set; }
        public MultiSelectList Roles { get; set; }
        public string[] AssignedRoles { get; set; }
        //Used an array since it was simple items and no objects being used

    }
}