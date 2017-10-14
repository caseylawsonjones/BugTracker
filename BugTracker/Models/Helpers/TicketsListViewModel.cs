using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models.Helpers {
    public class TicketsListViewModel {
        // This class helps to pass a List of Project.Users to the
        // Tickets/Create view to provide a drop-down menu for assigning users.
        public ICollection<Ticket> AllTickets { get; set; }
        public ICollection<Ticket> ProjectTickets { get; set; }
        public ICollection<Ticket> AssignedTickets { get; set; }
        public ICollection<Ticket> SubmittedTickets { get; set; }
        public ICollection<ApplicationUser> Users { get; set; }
    }
}