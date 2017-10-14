using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class TicketPriority {

        public int Id { get; set; }
        [Display(Name = "Priority Level")]
        public string Name { get; set; }

    }
}