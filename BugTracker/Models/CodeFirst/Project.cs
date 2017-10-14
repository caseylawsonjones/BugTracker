using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class Project {

        public Project() {
            Users = new HashSet<ApplicationUser>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }

        [Display(Name = "Creation Date")]
        public DateTimeOffset Created { get; set; }
        [Display(Name = "Last Updated")]
        public DateTimeOffset? Updated { get; set; }
        [Display(Name = "Project Title")]
        public string Title { get; set; }
        [Display(Name = "Project Description")]
        public string Description { get; set; }
        [Display(Name = "Project Owner")]
        public string AuthorId { get; set; }

        //public bool? IsArchived { get; set; }

        //No virtual relationship is created for AuthorId because that would define
        //a second type of relationship of one to many when the statement below
        //already defines a many to many relationship
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }

    }
}