using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BugTracker.Models.CodeFirst {
    public class TicketAttachment {

        public int Id { get; set; }
        [Display(Name = "Attachment Description")]
        public string Description { get; set; }
        [Display(Name = "Attachment File URL")]
        public string FileUrl { get; set; }
        [Display(Name = "Attachment File Name")]
        public string FileName { get; set; }
        [Display(Name = "Creation Date")]
        public DateTimeOffset Created { get; set; }
        [Display(Name = "Icon URL")]
        public string IconUrl { get; set; }
        // Icons used come from https://github.com/redbooth/free-file-icons

        //Foreign Keys
        [Display(Name = "Parent Ticket")]
        public int TicketId { get; set; }
        [Display(Name = "Attachment Creator")]
        public string AuthorId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Author { get; set; }

    }
}