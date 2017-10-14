using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class TicketComment {

        public int Id { get; set; }
        [Display(Name = "Comment Body")]
        public string Body { get; set; }
        [Display(Name = "Creation Date")]
        public DateTimeOffset Created { get; set; }
        [Display(Name = "Last Updated")]
        public DateTimeOffset? Updated { get; set; }

        // Foreign Keys
        [Display(Name = "Parent Ticket")]
        public int TicketId { get; set; }
        [Display(Name = "Comment Author")]
        public string AuthorId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Author { get; set; }


    }
}