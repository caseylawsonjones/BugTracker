using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Helpers {

    public class ProjectsListViewModel : Universal {

        public ICollection<Project> AllProjects { get; set; }
        public ICollection<Ticket> AllTickets { get; set; }
        public ICollection<ApplicationUser> AllUsers { get; set; }
        //public ApplicationUser[] Developers { get; set; }
        //public ICollection<Ticket> ManagedProjects { get; set; } - this may be used later if Projects are assigned a specific PM

        // Lists for All of the Current User's Assigned Projects and Tickets
        public ICollection<Project> UserProjects { get; set; }
        public ICollection<Ticket> UserTickets { get; set; }
        public ICollection<Ticket> SubmittedTickets { get; set; }

        public ICollection<ApplicationUser> ProjectAuthors { get; set; }

        // Lists of Users with either No Roles assigned, or no Ticket Assignments
        public ICollection<ApplicationUser> UsersWithNoRoles { get; set; }
        public ICollection<ApplicationUser> DevelopersWithNoTicketAssignments { get; set; }

        //public Dictionary<string, string[]> AllProjectsTicketUserChartData { get; set; }
        //public Dictionary<string, string[]> UserProjectsTicketUserChartData { get; set; }
        //public string[,] AllProjectsTicketUserChartDataArray { get; set; }
        //public string[,] UserProjectsTicketUserChartDataArray { get; set; }
    }
}