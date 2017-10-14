using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class ProjectHistory {

        public int Id { get; set; }
        [Display(Name = "Modification Type")]
        public string Property { get; set; }
        [Display(Name = "Previous Value")]
        public string OldValue { get; set; }
        [Display(Name = "New Value")]
        public string NewValue { get; set; }
        [Display(Name = "Modification Date")]
        public DateTimeOffset Created { get; set; }

        // Foreign Keys
        [Display(Name = "Modified by")]
        public string AuthorId { get; set; }
        [Display(Name = "Modified Ticket")]
        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Author { get; set; }

    }
}