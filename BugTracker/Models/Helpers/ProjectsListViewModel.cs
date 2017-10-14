using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Helpers {

    public class ProjectsListViewModel : Universal {

        public ICollection<Project> AllProjects { get; set; }
        //public ICollection<Ticket> ManagedProjects { get; set; } - this may be used later if Projects are assigned a specific PM
        public ICollection<Project> UserProjects { get; set; }
        public ICollection<ApplicationUser> ProjectAuthors { get; set; }
    }
}