using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class Ticket {

        public Ticket() {
            Attachments = new HashSet<TicketAttachment>();
            TicketComments = new HashSet<TicketComment>();
            TicketHistories = new HashSet<TicketHistory>();
        }

        //Properties
        public int Id { get; set; }

        [Display(Name = "Creation Date")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        public DateTimeOffset Created { get; set; }

        [Display(Name = "Last Updated")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy/MM/dd hh:mm tt}")]
        public DateTimeOffset? Updated { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsArchived { get; set; }


        //Foreign Keys
        [Display(Name = "Parent Project")]
        public int ProjectId { get; set; }
        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }
        [Display(Name = "Ticket Priority")]
        public int TicketPriorityId { get; set; }
        [Display(Name = "Ticket Status")]
        public int TicketStatusId { get; set; }
        [Display(Name = "Ticket Owner")]
        public string OwnerUserId { get; set; }
        [Display(Name = "Assigned Developer")]
        public string AssignedUserId { get; set; }

        public virtual Project Project { get; set; }
        public virtual TicketType TicketType { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual ApplicationUser OwnerUser{ get; set; }
        public virtual ApplicationUser AssignedUser { get; set; }

        //We cannot setup a Many-to-Many relationship with ApplicationUser because we have two existing One-to-Many relationships

        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketComment> TicketComments { get; set; }
        public virtual ICollection<TicketHistory> TicketHistories { get; set; }
        
    }
}