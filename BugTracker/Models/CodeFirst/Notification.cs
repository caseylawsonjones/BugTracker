using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class Notification {

        public int Id { get; set; }

        //[Display(Name = "Modification Type")]
        //public string Property { get; set; }
        //[Display(Name = "Previous Value")]
        //public string OldValue { get; set; }
        //[Display(Name = "New Value")]
        //public string NewValue { get; set; }

        [Display(Name = "Created Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Ticket Modified by")]
        public string AuthorName { get; set; }

        [Display(Name = "Notification Message")]
        public string Body { get; set; }

        [Display(Name = "Read")]
        public bool IsRead { get; set; }

        // Foreign Keys
        [Display(Name = "Modifier's User ID")]
        public string AuthorId { get; set; }

        [Display(Name = "Modified Ticket")]
        public int TicketId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}