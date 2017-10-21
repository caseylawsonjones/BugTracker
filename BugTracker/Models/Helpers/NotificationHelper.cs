using BugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;
using System.Data.Entity;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;

namespace BugTracker.Models.Helpers {
    public class NotificationHelper : Universal {

        // User.Identity.GetUserId() will not work in the helper class that this was templated from (HistoryHellper.cs)
        // for some unknown reason.  To compensate for this, the userId is being passed during instantiation.
        private string _userId;

        public NotificationHelper(string userId) {
            _userId = userId;
        }

        public async Task AddNotification(int ticketId, string[,] arrChangedProperties, int arrayLength, string toEmailAddress) {
            Notification notification = new Notification();
            string authorName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            notification.AuthorId = _userId;
            notification.AuthorName = authorName;
            notification.TicketId = ticketId;
            notification.Created = DateTimeOffset.UtcNow;
            Ticket ticket = db.Tickets.Find(ticketId);

            StringBuilder message = new StringBuilder();
            //Add first part of the message, down through the first table heading - that is always the same.
            message.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><title>Ticket Modified</title><style>*{word-wrap:keep-all ;}table {table-layout:fixed;min-width:100%;max-width:100%;}td {padding-right:25px;}th {text-align:left;text-decoration:underline;}body(background-color:white;}h1 {color:white;margin-bottom:0px;}h2 {color:white;margin-bottom:0px;}h3 {margin-top:5px;}h4 {color:white;margin-top:5px;}</style></head><body><table><tbody><tr style='background-color:#4717f6;'><td><h1 style='color:white;margin-bottom:5px;'>BugTracker Notification Services</h1><h4>Ticket Change Notification</h4></td></tr></tbody></table><h2 style='color:black;'>Ticket Modified</h2><h3>A ticket that you are assigned to has been modified.</h3><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Overview</h2></td></tr></tbody></table><h3>Here are the current ticket details:</h3><table><thead><th>Property</th><th>Current Value</th></thead>");
            //Body of the first table
            message.Append("<tbody><tr style='background-color:lightblue;'><td>Link</td><td><a href='https://cjones-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "'>/Tickets/Details/" + ticket.Id + "</a></td></tr><tr><td>Title</td><td>" + ticket.Title + "</td></tr><tr style='background-color:lightblue;'><td>Description</td><td>" + ticket.Description + "</td></tr><tr><td>Project</td><td>" + ticket.Project.Title + "</td></tr><tr style='background-color:lightblue;'><td>Priority</td><td>" + ticket.TicketPriority.Name + "</td></tr><tr><td>Status</td><td>" + ticket.TicketStatus.Name + "</td></tr><tr style='background-color:lightblue;'><td>Type</td><td>" + ticket.TicketType.Name + "</td></tr><tr><td>Creation On</td><td>" + ticket.Created + "</td></tr><tr style='background-color:lightblue;'><td>Last Updated</td><td>" + ticket.Updated + "</td></tr><tr><td>Owner</td><td>" + ticket.OwnerUser.FullName + "</td></tr><tr style='background-color:lightblue;'><td>Developer</td><td>" + ticket.AssignedUser.FullName + "</td></dl></tr></tbody></table>");
            //Add the ticket's current property values
            //message.Append("<dt>Ticket Title</dt><dd>" + ticket.Title + "</dd><dt>Ticket Description</dt><dd>" + ticket.Description + "</dd><dt>Project</dt><dd>" + ticket.Project.Title + "</dd><dt>Priority Level</dt><dd>" +  ticket.TicketPriority.Name + "</dd><dt>Status Level</dt><dd>" + ticket.TicketStatus.Name + "</dd><dt>Ticket Type</dt><dd>" + ticket.TicketType.Name + "</dd><dt>Creation Date</dt><dd>" + ticket.Created + "</dd><dt>Last Updated</dt><dd>" + ticket.Updated + " </dd><dt>Ticket Owner</dt><dd>" + ticket.OwnerUser.FullName + "</dd><dt>Assigned Developer</dt><dd>" + ticket.AssignedUser.FullName + "</dd></dl>");
            //Everything through the Modified Properties table header and opening <tbody> tag
            message.Append("<br><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Modification Details</h2></td></tr></tbody></table><table><tbody></tr><tr><td>Modified by:</td><td>" + authorName + "</td></tr><tr><td>Modified On:</td><td>" + ticket.Updated + "</td></tbody></table><table><h3>Modified Properties</h3><thead><tr><th>Property</th><th>Old Value</th><th>New Value</th></tr></thead><tbody>");
            //Add the modified values, from arrChangedProperties
            for (int i = 0; i < arrayLength; i++) { //arrayLength is too long, therefore "<", not "<="
                if((i != 0) && (i%2 != 0)) {  //The 0 row and every other row will have a background color of "lightblue".
                message.Append("<tr><td>" + arrChangedProperties[i, 0] + "</td><td>" + arrChangedProperties[i, 1] + "</td><td>" + arrChangedProperties[i, 2] + "</td></tr>");
                }
                else {
                    message.Append("<tr style='background-color:lightblue;'><td>" + arrChangedProperties[i, 0] + "</td><td>" + arrChangedProperties[i, 1] + "</td><td>" + arrChangedProperties[i, 2] + "</td></tr>");
                }
            }
            //Add the HTML for the end of the document
            message.Append("</tbody></table></body></html>");

            notification.Body = message.ToString();
            await SendNotification(toEmailAddress, notification.Body, "Ticket Change Notification (do not reply)");

            db.Notifications.Add(notification);
            db.SaveChanges();
        }

        public async Task AddAttachmentNotification(int ticketId, List<string> attachments, bool attAdded, string toEmailAddress) {
            Notification notification = new Notification();
            string authorName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            notification.AuthorId = _userId;
            notification.AuthorName = authorName;
            notification.TicketId = ticketId;
            notification.Created = DateTimeOffset.UtcNow;
            Ticket ticket = db.Tickets.Find(ticketId);
            string attSingleOrPlural = "An attachment has";
            if (attAdded) {//Only one attachment can be deleted at at time, but multiple can be uploaded at a time
                if (attachments.Count > 1) {
                    attSingleOrPlural = "Attachments have";
                }
            }
            string attachmentStatus = "deleted from";
            string attachmentStatus2 = "Deleted";
            if (attAdded == true) {
                attachmentStatus = "added to";
                attachmentStatus2 = "Added";
            }

            StringBuilder message = new StringBuilder();
            //Add first part of the message, down through the first table heading - that is always the same.
            message.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><title>Ticket Modified</title><style>* {word-wrap:keep-all ;}table {table-layout:fixed;min-width:100%;max-width:100%;}td {padding-right:25px;}th {text-align:left;text-decoration:underline;}body(background-color:white;}h1 {color:white;margin-bottom:0px;}h2 {color:white;margin-bottom:0px;}h3 {margin-top:5px;}h4 {color:white;margin-top:5px;}</style></head><body><table><tbody><tr style='background-color:#4717f6;'><td><h1 style='color:white;margin-bottom:5px;'>BugTracker Notification Services</h1><h4>Ticket Attachment Notification</h4></td></tr></tbody></table><h2 style='color:black;'>Ticket Modified</h2><h3>" + attSingleOrPlural + " been " + attachmentStatus + " one of your assigned tickets.</h3><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Overview</h2></td></tr></tbody></table><h3>Here are the current ticket details:</h3><table><thead><th>Property</th><th>Current Value</th></thead>");
            //Body of the first table
            message.Append("<tbody><tr style='background-color:lightblue;'><td>Link</td><td><a href='https://cjones-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "'>/Tickets/Details/" + ticket.Id + "</a></td></tr><tr><td>Title</td><td>" + ticket.Title + "</td></tr><tr style='background-color:lightblue;'><td>Description</td><td>" + ticket.Description + "</td></tr><tr><td>Project</td><td>" + ticket.Project.Title + "</td></tr><tr style='background-color:lightblue;'><td>Priority</td><td>" + ticket.TicketPriority.Name + "</td></tr><tr><td>Status</td><td>" + ticket.TicketStatus.Name + "</td></tr><tr style='background-color:lightblue;'><td>Type</td><td>" + ticket.TicketType.Name + "</td></tr><tr><td>Creation Date</td><td>" + ticket.Created + "</td></tr><tr style='background-color:lightblue;'><td>Last Updated</td><td>" + ticket.Updated + "</td></tr><tr><td>Owner</td><td>" + ticket.OwnerUser.FullName + "</td></tr><tr style='background-color:lightblue;'><td>Developer</td><td>" + ticket.AssignedUser.FullName + "</td></dl></tr></tbody></table>");
            //Add the ticket's current property values
            //message.Append("<dt>Ticket Title</dt><dd>" + ticket.Title + "</dd><dt>Ticket Description</dt><dd>" + ticket.Description + "</dd><dt>Project</dt><dd>" + ticket.Project.Title + "</dd><dt>Priority Level</dt><dd>" +  ticket.TicketPriority.Name + "</dd><dt>Status Level</dt><dd>" + ticket.TicketStatus.Name + "</dd><dt>Ticket Type</dt><dd>" + ticket.TicketType.Name + "</dd><dt>Creation Date</dt><dd>" + ticket.Created + "</dd><dt>Last Updated</dt><dd>" + ticket.Updated + " </dd><dt>Ticket Owner</dt><dd>" + ticket.OwnerUser.FullName + "</dd><dt>Assigned Developer</dt><dd>" + ticket.AssignedUser.FullName + "</dd></dl>");
            //Everything through the Modified Properties table header and opening <tbody> tag
            message.Append("<br><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Attachment Details</h2></td></tr></tbody></table><table><tbody></tr><tr><td>" + attachmentStatus2 + " by:</td><td>" + authorName + "</td></tr><tr><td>Date " + attachmentStatus2 + ":</td><td>" + ticket.Updated + "</td></tbody></table><table><h3>Attachment(s) " + attachmentStatus2 + ":</h3><tbody>");
            //Add the modified values, from arrChangedProperties
            int i = 0;
            foreach(var file in attachments) {
                if ((i != 0) && (i % 2 != 0)) {  //The 0 row and every other row will have a background color of "lightblue".
                    message.Append("<tr><td>" + file + "</td></tr>");
                }
                else {
                    message.Append("<tr style='background-color:lightblue;'><td>" + file + "</td></tr>");
                }
                i++;
            }
            //Add the HTML for the end of the document
            message.Append("</tbody></table></body></html>");

            notification.Body = message.ToString();
            await SendNotification(toEmailAddress, notification.Body, "Ticket Change Notification (do not reply)");

            db.Notifications.Add(notification);
            db.SaveChanges();
        }

        public async Task AddCommentNotification(int ticketId, string comment, bool commentAdded, string toEmailAddress) {
            Notification notification = new Notification();
            string authorName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            notification.AuthorId = _userId;
            notification.AuthorName = authorName;
            notification.TicketId = ticketId;
            notification.Created = DateTimeOffset.UtcNow;
            Ticket ticket = db.Tickets.Find(ticketId);
            
            string commentStatus = "deleted from";
            string commentStatus2 = "Deleted";
            if (commentAdded == true) {
                commentStatus = "added to";
                commentStatus2 = "Added";
            }

            StringBuilder message = new StringBuilder();
            //Add first part of the message, down through the first table heading - that is always the same.
            message.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><title>Ticket Modified</title><style>* {word-wrap:keep-all ;}table {table-layout:fixed;min-width:100%;max-width:100%;}td {padding-right:25px;}th {text-align:left;text-decoration:underline;}body(background-color:white;}h1 {color:white;margin-bottom:0px;}h2 {color:white;margin-bottom:0px;}h3 {margin-top:5px;}h4 {color:white;margin-top:5px;}</style></head><body><table><tbody><tr style='background-color:#4717f6;'><td><h1 style='color:white;margin-bottom:5px;'>BugTracker Notification Services</h1><h4>Ticket Comment Notification</h4></td></tr></tbody></table><h2 style='color:black;'>Ticket Modified</h2><h3>A comment has been " + commentStatus + " one of your assigned tickets..</h3><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Overview</h2></td></tr></tbody></table><h3>Here are the current ticket details:</h3><table><thead><th>Property</th><th>Current Value</th></thead>");
            //Body of the first table
            message.Append("<tbody><tr style='background-color:lightblue;'><td>Link</td><td><a href='https://cjones-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "'>/Tickets/Details/" + ticket.Id + "</a></td></tr><tr><td>Title</td><td>" + ticket.Title + "</td></tr><tr style='background-color:lightblue;'><td>Description</td><td>" + ticket.Description + "</td></tr><tr><td>Project</td><td>" + ticket.Project.Title + "</td></tr><tr style='background-color:lightblue;'><td>Priority</td><td>" + ticket.TicketPriority.Name + "</td></tr><tr><td>Status</td><td>" + ticket.TicketStatus.Name + "</td></tr><tr style='background-color:lightblue;'><td>Type</td><td>" + ticket.TicketType.Name + "</td></tr><tr><td>Creation Date</td><td>" + ticket.Created + "</td></tr><tr style='background-color:lightblue;'><td>Last Updated</td><td>" + ticket.Updated + "</td></tr><tr><td>Owner</td><td>" + ticket.OwnerUser.FullName + "</td></tr><tr style='background-color:lightblue;'><td>Developer</td><td>" + ticket.AssignedUser.FullName + "</td></dl></tr></tbody></table>");
            //Add the ticket's current property values
            //message.Append("<dt>Ticket Title</dt><dd>" + ticket.Title + "</dd><dt>Ticket Description</dt><dd>" + ticket.Description + "</dd><dt>Project</dt><dd>" + ticket.Project.Title + "</dd><dt>Priority Level</dt><dd>" +  ticket.TicketPriority.Name + "</dd><dt>Status Level</dt><dd>" + ticket.TicketStatus.Name + "</dd><dt>Ticket Type</dt><dd>" + ticket.TicketType.Name + "</dd><dt>Creation Date</dt><dd>" + ticket.Created + "</dd><dt>Last Updated</dt><dd>" + ticket.Updated + " </dd><dt>Ticket Owner</dt><dd>" + ticket.OwnerUser.FullName + "</dd><dt>Assigned Developer</dt><dd>" + ticket.AssignedUser.FullName + "</dd></dl>");
            //Everything through the Modified Properties table header and opening <tbody> tag
            message.Append("<br><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Comment Details</h2></td></tr></tbody></table><table><tbody></tr><tr><td>" + commentStatus2 + " by:</td><td>" + authorName + "</td></tr><tr><td>Date " + commentStatus2 + ":</td><td>" + ticket.Updated + "</td></tbody></table><table><h3>Comment " + commentStatus2 + ":</h3><tbody>");
            //Add the info for the modified comment
            message.Append("<tr style='background-color:lightblue;'><td>&quot;" + comment + "&quot;</td></tr>");
            //Add the HTML for the end of the document
            message.Append("</tbody></table></body></html>");

            notification.Body = message.ToString();
            await SendNotification(toEmailAddress, notification.Body, "Ticket Change Notification (do not reply)");

            db.Notifications.Add(notification);
            db.SaveChanges();
        }

        public async Task AddCommentEditedNotification(int ticketId, string oldValue, string newValue, string toEmailAddress) {
            Notification notification = new Notification();
            string authorName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
            notification.AuthorId = _userId;
            notification.AuthorName = authorName;
            notification.TicketId = ticketId;
            notification.Created = DateTimeOffset.UtcNow;
            Ticket ticket = db.Tickets.Find(ticketId);

            StringBuilder message = new StringBuilder();
            //Add first part of the message, down through the first table heading - that is always the same.
            message.Append("<!DOCTYPE html><html><head><meta charset='utf-8' /><title>Ticket Modified</title><style>* {word-wrap:keep-all ;}table {table-layout:fixed;min-width:100%;max-width:100%;}td {padding-right:25px;}th {text-align:left;text-decoration:underline;}body(background-color:white;}h1 {color:white;margin-bottom:0px;}h2 {color:white;margin-bottom:0px;}h3 {margin-top:5px;}h4 {color:white;margin-top:5px;}</style></head><body><table><tbody><tr style='background-color:#4717f6;'><td><h1 style='color:white;margin-bottom:5px;'>BugTracker Notification Services</h1><h4>Ticket Comment Notification</h4></td></tr></tbody></table><h2 style='color:black;'>Ticket Modified</h2><h3>A comment has been edited on one of your assigned tickets..</h3><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Overview</h2></td></tr></tbody></table><h3>Here are the current ticket details:</h3><table><thead><th>Property</th><th>Current Value</th></thead>");
            //Body of the first table
            message.Append("<tbody><tr style='background-color:lightblue;'><td>Link</td><td><a href='https://cjones-bugtracker.azurewebsites.net/Tickets/Details/" + ticket.Id + "'>/Tickets/Details/" + ticket.Id + "</a></td></tr><tr><td>Title</td><td>" + ticket.Title + "</td></tr><tr style='background-color:lightblue;'><td>Description</td><td>" + ticket.Description + "</td></tr><tr><td>Project</td><td>" + ticket.Project.Title + "</td></tr><tr style='background-color:lightblue;'><td>Priority</td><td>" + ticket.TicketPriority.Name + "</td></tr><tr><td>Status</td><td>" + ticket.TicketStatus.Name + "</td></tr><tr style='background-color:lightblue;'><td>Type</td><td>" + ticket.TicketType.Name + "</td></tr><tr><td>Creation Date</td><td>" + ticket.Created + "</td></tr><tr style='background-color:lightblue;'><td>Last Updated</td><td>" + ticket.Updated + "</td></tr><tr><td>Owner</td><td>" + ticket.OwnerUser.FullName + "</td></tr><tr style='background-color:lightblue;'><td>Developer</td><td>" + ticket.AssignedUser.FullName + "</td></dl></tr></tbody></table>");
            //Add the ticket's current property values
            //message.Append("<dt>Ticket Title</dt><dd>" + ticket.Title + "</dd><dt>Ticket Description</dt><dd>" + ticket.Description + "</dd><dt>Project</dt><dd>" + ticket.Project.Title + "</dd><dt>Priority Level</dt><dd>" +  ticket.TicketPriority.Name + "</dd><dt>Status Level</dt><dd>" + ticket.TicketStatus.Name + "</dd><dt>Ticket Type</dt><dd>" + ticket.TicketType.Name + "</dd><dt>Creation Date</dt><dd>" + ticket.Created + "</dd><dt>Last Updated</dt><dd>" + ticket.Updated + " </dd><dt>Ticket Owner</dt><dd>" + ticket.OwnerUser.FullName + "</dd><dt>Assigned Developer</dt><dd>" + ticket.AssignedUser.FullName + "</dd></dl>");
            //Everything through the Modified Properties table header and opening <tbody> tag
            message.Append("<br><table><tbody><tr style='background-color:#4717f6;'><td><h2 style='margin-bottom:11px;'>Comment Details</h2></td></tr></tbody></table><table><tbody></tr><tr><td>Edited by:</td><td>" + authorName + "</td></tr><tr><td>Edit Date:</td><td>" + ticket.Updated + "</td></tbody></table><table><h3>Comment Values:</h3><tbody>");
            //Add the info for the modified comment
            message.Append("<tr style='background-color:#4717f6;'><td><span style='color:white;'>Old Comment</span></td></tr><tr style='background-color:lightblue;'><td>&quot;" + oldValue + "&quot;</td></tr><tr style='background-color:#ffffff;'><td><span style=color:#ffffff; text-align:center;>|||||</span></td></tr><tr style='background-color:#4717f6;'><td><span style='color:white;'>New Comment</span></td><tr style='background-color:#90EE90;'><td>&quot;" + newValue + "&quot;</td></tr>");
            //Add the HTML for the end of the document
            message.Append("</tbody></table></body></html>");

            notification.Body = message.ToString();
            await SendNotification(toEmailAddress, notification.Body, "Ticket Change Notification (do not reply)");

            db.Notifications.Add(notification);
            db.SaveChanges();
        }

        private async Task SendNotification(string toEmailAddress, string emailBody, string emailSubject) {
            try {
                var fromEmailAddress = "BugTracker<webappmessages@gmail.com>";
                //var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
                //model.Body = "This is a message from your BugTracker web application.  The name and the email of the contacting person is above.";
                var email = new MailMessage(fromEmailAddress, toEmailAddress) {
                    Subject = emailSubject,
                    Body = emailBody,
                    IsBodyHtml = true
                };
                var svc = new PersonalEmail();
                await svc.SendAsync(email);
                return;
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                await Task.FromResult(0);
            }
        }

        //public void AddNotification(int ticketId, string changedProperty, int oldValue, int newValue) {
        //    Notification notification = new Notification();
        //    string authorName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
        //    notification.AuthorId = _userId;
        //    notification.AuthorName = authorName;
        //    notification.TicketId = ticketId;
        //    notification.Property = changedProperty;
        //    notification.OldValue = oldValue.ToString();
        //    notification.NewValue = newValue.ToString();
        //    notification.Created = DateTimeOffset.UtcNow;
        //    db.Notifications.Add(notification);
        //    db.SaveChanges();
        //}

        //public void AddNotification(int ticketId, string changedProperty, bool oldValue, bool newValue) {
        //    Notification notification = new Notification();
        //    string authorName = db.Users.AsNoTracking().First(t => t.Id == _userId).FullName;
        //    notification.AuthorId = _userId;
        //    notification.AuthorName = authorName;
        //    notification.TicketId = ticketId;
        //    notification.Property = changedProperty;
        //    notification.OldValue = oldValue.ToString();
        //    notification.NewValue = newValue.ToString();
        //    notification.Created = DateTimeOffset.UtcNow;
        //    db.Notifications.Add(notification);
        //    db.SaveChanges();
        //}
    }
}