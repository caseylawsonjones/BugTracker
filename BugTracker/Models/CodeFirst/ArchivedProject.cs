using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class ArchivedProject {
        // This class is for archived projects and is read-only.
        public ArchivedProject() {
            Users = new HashSet<ApplicationUser>();
            Tickets = new HashSet<Ticket>();
        }

        public int Id { get; set; }

        [Display(Name = "Creation Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Last Updated")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        public DateTimeOffset? Updated { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        [Display(Name = "Creator")]
        public string AuthorId { get; set; }

        //No virtual relationship is created for AuthorId because that would define
        //a second type of relationship of one to many when the statement below
        //already defines a many to many relationship
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }

    }
}