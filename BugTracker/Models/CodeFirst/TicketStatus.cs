using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class TicketStatus {

        public int Id { get; set; }
        [Display(Name = "Status Level")]
        public string Name { get; set; }

    }
}