using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.Helpers {
    public class ProjectUserViewModel : Universal {
        public int ProjectId { get; set; }
        public string ProjectOwner { get; set; }
        public Project Project { get; set; }
        public List<ApplicationUser> AssignedUsersList { get; set; }
        public string[] AssignedUsers { get; set; }      //Users already assigned to the project
        public MultiSelectList Users { get; set; }       //A MultiSelectList of all users which will have the AssignedUser Users highlighted
    }
}