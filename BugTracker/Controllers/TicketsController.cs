using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using BugTracker.Models.Helpers;
using System.IO;
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class TicketsController : Universal
    {
        // GET: Tickets
        public ActionResult Index()
        {
            TicketsListViewModel tListVM = new TicketsListViewModel();
            ICollection<Ticket> tickets = db.Tickets.Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType).ToList();
            tListVM.Users = db.Users.ToList();
            ViewBag.NoRolesAssigned = false;
            tListVM.AllTickets = tickets.Where(t => t.IsArchived == false).ToList();

            // Admin Role does not need to be checked as all tickets have already been passed to the VM.
            if (User.IsInRole("ProjectManager")) {
                List<ApplicationUser> users = db.Users.ToList();
                ApplicationUser user = users.First(u => u.Id == User.Identity.GetUserId());
                List<Project> projects = user.Projects.ToList();
                List<Ticket> ticketsPM = new List<Ticket>();
                foreach (var project in projects) {
                    foreach (var ticket in project.Tickets) {
                        if (!ticket.IsArchived) {
                            ticketsPM.Add(ticket);
                        }
                    }
                }
                tListVM.ProjectTickets = ticketsPM;
            }

            if (User.IsInRole("Developer")) {
                List<Ticket> ticketsDev = tickets.Where(t => (t.AssignedUserId == User.Identity.GetUserId()) && t.IsArchived == false).ToList();
                tListVM.AssignedTickets = ticketsDev;
            }

            if (User.IsInRole("Submitter")) {
                List<Ticket> ticketsSub = tickets.Where(t => (t.OwnerUserId == User.Identity.GetUserId()) && t.IsArchived == false).ToList();
                tListVM.SubmittedTickets = ticketsSub;
            }
            if (tListVM == null) {
                ViewBag.NoRolesAssigned = true;
            }
            return View(tListVM);
        }

        // GET: Tickets/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null) {
                return HttpNotFound();
            }
                // Verifying that the current user is involved with this project
                var project = db.Projects.First(p => p.Id == ticket.ProjectId);
                if (User.IsInRole("Admin") ||
                    (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                    (User.IsInRole("Developer")      && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                    (User.IsInRole("Submitter")      && ticket.OwnerUserId == User.Identity.GetUserId())) {
                ViewBag.ownerUser = db.Users.Find(ticket.OwnerUserId).FullName;
                if (ticket.AssignedUserId != null) {
                    ViewBag.assignedUser = db.Users.Find(ticket.AssignedUserId).FullName;
                }
                    return View(ticket);
                }
                else {
                    return RedirectToAction("Index", "Tickets");
                }

        }

        // GET: Tickets/Create
        [Authorize (Roles="Submitter")]
        public ActionResult Create()
        {
            List<Project> projects = db.Projects.ToList();
            ViewBag.ProjectId = new SelectList(projects.Where(p => p.Users.Any(u => u.Id == User.Identity.GetUserId())).ToList(), "Id", "Title");
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Submitter")]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket, ICollection<HttpPostedFileBase> files)
        {
            if (ModelState.IsValid)
            {
                ticket.Created = DateTimeOffset.UtcNow;
                ticket.OwnerUserId = User.Identity.GetUserId();
                ticket.TicketStatusId = 1;
                ticket.IsArchived = false;
                db.Tickets.Add(ticket);
                db.SaveChanges();

                //Now create a folder for any attachments to be added
                var attPath = "~/Content/Media/Ticket Attachments/" + ticket.Id;
                var newPath = Server.MapPath(attPath);
                Directory.CreateDirectory(newPath);


                // Now save the attachments
                TicketAttachment attachment = new TicketAttachment();
                foreach(var att in files) {
                    if (att != null && att.ContentLength > 0) {

                        // Process File Info and Save File
                        var ext = Path.GetExtension(att.FileName).ToLower(); // Gets image's extension and then sets it to lower case
                        var filePath = "Content/Media/Ticket Attachments/" + ticket.Id;
                        var absPath = Server.MapPath("~/" + filePath);
                        string newFileName = att.FileName;
                        var num = 0;
                        while (System.IO.File.Exists(Path.Combine(absPath, newFileName))) {
                            //Sets "filename" back to the default value
                            newFileName = Path.GetFileNameWithoutExtension(att.FileName);
                            //Add's parentheses after the name with a number ex. filename(4)
                            newFileName = string.Format(newFileName + "(" + ++num + ")" + ext);
                            //Makes sure pPic gets updated with the new filename so it could check
                            //attach = fileName + Path.GetExtension(doc.FileName);
                        }
                        att.SaveAs(Path.Combine(absPath, newFileName));

                        // Update attachment info and add to database
                        attachment.FileUrl = "/" + filePath + "/" + newFileName;
                        attachment.FileName = newFileName;
                        attachment.AuthorId = User.Identity.GetUserId();
                        attachment.Created = DateTime.UtcNow;
                        attachment.TicketId = ticket.Id;
                        attachment.IconUrl = "";
                        switch (ext) {
                            case ".png":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/png.png";
                                break;
                            case ".jpg":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/jpg.png";
                                break;
                            case ".jpeg":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/jpeg.png";
                                break;
                            case ".gif":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/gif.png";
                                break;
                            case ".bmp":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/bmp.png";
                                break;
                            case ".jfif":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/jfif.png";
                                break;
                            case ".tiff":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/tiff.png";
                                break;
                            case ".doc":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/doc.png";
                                break;
                            case ".docx":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/doc.png";
                                break;
                            case ".xls":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/xls.png";
                                break;
                            case ".xlsx":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/xls.png";
                                break;
                            case ".pdf":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/pdf.png";
                                break;
                            case ".txt":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/txt.png";
                                break;
                            case ".htm":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/html.png";
                                break;
                            case ".html":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/html.png";
                                break;
                            case ".ppt":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/ppt.png";
                                break;
                            case ".pptx":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/ppt.png";
                                break;
                            case ".zip":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/zip.png";
                                break;
                            case ".rar":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/rar.png";
                                break;
                            case ".csv":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/csv.png";
                                break;
                            case ".rtf":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/rtf.png";
                                break;
                            case ".xml":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/xml.png";
                                break;
                            case ".mp3":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/mp3.png";
                                break;
                            case ".mp4":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/mp4.png";
                                break;
                            case ".wav":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/wav.png";
                                break;
                            case ".avi":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/avi.png";
                                break;
                            case ".wmv":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/_blank.png";
                                break;
                            case ".mpg":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/mpg.png";
                                break;
                            case ".flv":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/flv.png";
                                break;
                            case ".mov":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/mov.png";
                                break;
                            case ".css":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/css.png";
                                break;
                            case ".less":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/less.png";
                                break;
                            case ".sass":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/sass.png";
                                break;
                            case ".dat":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/dat.png";
                                break;
                            case ".cpp":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/cpp.png";
                                break;
                            case ".js":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/js.png";
                                break;
                            case ".py":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/py.png";
                                break;
                            case ".rb":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/rb.png";
                                break;
                            case ".php":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/php.png";
                                break;
                            case ".dwg":
                                attachment.IconUrl = "~/Content/Images/Icons/512px/dwg.png";
                                break;
                            default:
                                attachment.IconUrl = "~/Content/Images/Icons/512px/_blank.png";
                                break;
                        }
                        db.TicketAttachments.Add(attachment);
                        db.SaveChanges();
                    }
                }

                // Now add the TicketHistory for the Ticket Creation
                HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
                ticketHistory.AddHistoryEvent(ticket.Id, "CreationDate", null, ticket.Created.ToString());

                return RedirectToAction("Index");
            }
            List<Project> projects = db.Projects.ToList();
            ViewBag.ProjectId = new SelectList(projects.Where(p => p.Users.Any(u => u.Id == User.Identity.GetUserId())).ToList(), "Id", "Title");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            // Verifying that the current user is involved with this project
            var project = db.Projects.First(p => p.Id == ticket.ProjectId);
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                List<Project> projects = db.Projects.ToList();
                ViewBag.ProjectId = new SelectList(projects.Where(p => p.Users.Any(u => u.Id == User.Identity.GetUserId())).ToList(), "Id", "Title");
                //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
                ViewBag.TicketStatusId =   new SelectList(db.TicketStatuses,   "Id", "Name", ticket.TicketStatusId);
                ViewBag.TicketTypeId =     new SelectList(db.TicketTypes,      "Id", "Name", ticket.TicketTypeId);
                if(User.IsInRole("ProjectManager") && !User.IsInRole("Admin")) {
                    List<ApplicationUser> userList = project.Users.OrderBy(user => user.FirstName).ToList();
                    ViewBag.AssignedUserId = new SelectList(userList.Where(u => u.Roles.Any(r => (r.RoleId == "f6c4f96d-cab2-4667-b3da-70b9165f344c"))).OrderBy(user => user.FirstName).ToList(), "Id", "FullName", ticket.AssignedUserId);
                    // The referenced role is for "Developers".
                }
                if (User.IsInRole("Admin")) {
                    ViewBag.AssignedUserId = new SelectList(project.Users.Where(u => u.Roles.Any(r => (r.RoleId == "f6c4f96d-cab2-4667-b3da-70b9165f344c" || r.RoleId == "5e1d6394-7712-4389-bfee-eecc30b2b985"))).ToList(), "Id", "FullName", ticket.AssignedUserId);
                    // The referenced role is for "Developers".
                }
                    return View(ticket);
            }
            else {
                ViewBag.InvalidUser = true;
                return View();
            }
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Created,Updated,Title,Description,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignedUserId,IsArchived")] Ticket ticket)
        {
            var project = db.Projects.First(p => p.Id == ticket.ProjectId);
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {

                var userId = User.Identity.GetUserId();
                if (ModelState.IsValid) {
                    bool hasAssignedUserIdChanged = false;
                    // Declaration of a two-dimensional String Array and a Counter
                    // The array is set to the max possible size, which will likely not be used.
                    // The arrayCounter is passed, along with the array, to the NotificationHelper to indicate the # of rows in the array
                    string[,] arrChangedProperties = new string[7, 3];
                    int arrayCounter = 0;

                    // Ticket History entries need to be created for each event
                    // Determine which Property(s) were changed
                    HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
                    Ticket oldTicket = db.Tickets.AsNoTracking().First(t => t.Id == ticket.Id);
                    if (oldTicket.Title != ticket.Title) {
                        ticketHistory.AddHistoryEvent(ticket.Id, "Title", oldTicket.Title, ticket.Title);
                        arrChangedProperties[arrayCounter, 0] = "Title";
                        arrChangedProperties[arrayCounter, 1] = oldTicket.Title;
                        arrChangedProperties[arrayCounter, 2] = ticket.Title;
                        arrayCounter++;
                    }
                    if (oldTicket.Description != ticket.Description) {
                        ticketHistory.AddHistoryEvent(ticket.Id, "Description", oldTicket.Description, ticket.Description);
                        arrChangedProperties[arrayCounter, 0] = "Description";
                        arrChangedProperties[arrayCounter, 1] = oldTicket.Description;
                        arrChangedProperties[arrayCounter, 2] = ticket.Description;
                        arrayCounter++;
                    }
                    if (oldTicket.TicketPriorityId != ticket.TicketPriorityId) {
                        TicketPriority oldPriority = db.TicketPriorities.Find(oldTicket.TicketPriorityId);
                        TicketPriority newPriority = db.TicketPriorities.Find(ticket.TicketPriorityId);
                        ticketHistory.AddHistoryEvent(ticket.Id, "TicketPriorityId", oldPriority.Name, newPriority.Name);
                        arrChangedProperties[arrayCounter, 0] = "Priority";
                        arrChangedProperties[arrayCounter, 1] = oldPriority.Name;
                        arrChangedProperties[arrayCounter, 2] = newPriority.Name;
                        arrayCounter++;
                    }
                    if (oldTicket.TicketStatusId != ticket.TicketStatusId) {
                        TicketStatus oldStatus = db.TicketStatuses.Find(oldTicket.TicketStatusId);
                        TicketStatus newStatus = db.TicketStatuses.Find(ticket.TicketStatusId);
                        ticketHistory.AddHistoryEvent(ticket.Id, "TicketStatus", oldStatus.Name, newStatus.Name);
                        arrChangedProperties[arrayCounter, 0] = "Status";
                        arrChangedProperties[arrayCounter, 1] = oldStatus.Name;
                        arrChangedProperties[arrayCounter, 2] = newStatus.Name;
                        arrayCounter++;
                    }
                    if (oldTicket.AssignedUserId != ticket.AssignedUserId) {
                        ApplicationUser newUser = db.Users.Find(ticket.AssignedUserId);
                        arrChangedProperties[arrayCounter, 0] = "Developer";
                        if (oldTicket.AssignedUserId == null) {
                            ticketHistory.AddHistoryEvent(ticket.Id, "AssignedUserId", "null", newUser.FullName);
                            arrChangedProperties[arrayCounter, 1] = "null";
                        }
                        else {
                            ApplicationUser oldUser = db.Users.Find(oldTicket.AssignedUserId);
                            ticketHistory.AddHistoryEvent(ticket.Id, "AssignedUserId", oldUser.FullName, newUser.FullName);
                            arrChangedProperties[arrayCounter, 1] = oldUser.FullName;
                        }
                        arrChangedProperties[arrayCounter, 2] = newUser.FullName;
                        arrayCounter++;
                        hasAssignedUserIdChanged = true;
                    }
                    if (oldTicket.TicketTypeId != ticket.TicketTypeId) {
                        TicketType oldType = db.TicketTypes.Find(oldTicket.TicketTypeId);
                        TicketType newType = db.TicketTypes.Find(ticket.TicketTypeId);
                        ticketHistory.AddHistoryEvent(ticket.Id, "Ticket Type", oldType.Name, newType.Name);
                        arrChangedProperties[arrayCounter, 0] = "Type";
                        arrChangedProperties[arrayCounter, 1] = oldType.Name;
                        arrChangedProperties[arrayCounter, 2] = newType.Name;
                        arrayCounter++;

                    }
                    if (oldTicket.IsArchived != ticket.IsArchived) {
                        ticketHistory.AddHistoryEvent(ticket.Id, "Ticket Archived", oldTicket.IsArchived, ticket.IsArchived);
                        //hasIsArchivedChanged = true;
                        arrChangedProperties[arrayCounter, 0] = "Archived";
                        arrChangedProperties[arrayCounter, 1] = oldTicket.IsArchived.ToString();
                        arrChangedProperties[arrayCounter, 2] = ticket.IsArchived.ToString();
                        arrayCounter++;

                    }

                    //The ticket must be saved before calling the Notification
                    ticket.Updated = DateTimeOffset.UtcNow;
                    db.Entry(ticket).State = EntityState.Modified;
                    db.SaveChanges();

                    //Call NotificationHelper
                    NotificationHelper notification = new NotificationHelper(User.Identity.GetUserId());
                    string newAssignedUserEmailAddress = "temporary value";
                    if (ticket.AssignedUserId != null) {
                        newAssignedUserEmailAddress = db.Users.Find(ticket.AssignedUserId).Email;
                    }
                    string oldAssignedUserEmailAddress = "temporary value";
                    if (oldTicket.AssignedUserId != null) {
                        oldAssignedUserEmailAddress = db.Users.Find(oldTicket.AssignedUserId).Email;
                    }
                    if (User.Identity.GetUserId() == oldTicket.AssignedUserId) {//If the Current User was the Assignee and...
                        if (hasAssignedUserIdChanged) {//If the CurrentUser/oldAssignedUser has reassigned the ticket - Notification to newAssignedUser only
                            await notification.AddNotification(ticket.Id, arrChangedProperties, arrayCounter, newAssignedUserEmailAddress);
                        }//If the Current User did not reassign the Ticket - no Notifications
                    }
                    else {//If the Current User was NOT oldAssignedUser
                        if (User.Identity.GetUserId() == ticket.AssignedUserId) {
                            //If the Current User is newAssignedUser - Notification to oldAssignedUser only
                            //Assumes that the Assignee has changed since they are NOT the oldAssigneeId and ARE the newAssigneeId
                            if (oldTicket.AssignedUserId != null) {
                                await notification.AddNotification(ticket.Id, arrChangedProperties, arrayCounter, oldAssignedUserEmailAddress);
                            }
                        }
                        else {//Current User is neither the oldAssignedUser or the newAssignedUser
                            if (hasAssignedUserIdChanged) {
                                if (oldTicket.AssignedUserId != null) {
                                    await notification.AddNotification(ticket.Id, arrChangedProperties, arrayCounter, oldAssignedUserEmailAddress);
                                }
                                await notification.AddNotification(ticket.Id, arrChangedProperties, arrayCounter, newAssignedUserEmailAddress);
                            }
                            else {//Current User is neither assignee and the assignee has not changed.  Ticket changes were made, however and the assigned Developer needs to be notified.
                                await notification.AddNotification(ticket.Id, arrChangedProperties, arrayCounter, newAssignedUserEmailAddress);
                            }
                        }
                    }
                    //My God, it is finally over!!
                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
                }
                ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
                ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
                ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
                ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
                return View(ticket);
            }
            return RedirectToAction("Index");
        }

        ////GET: Tickets/Delete/5
        //public ActionResult Delete(int? id) {
        //    if (id == null) {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Ticket ticket = db.Tickets.Find(id);
        //    Project project = db.Projects.Find(ticket.ProjectId);
        //    if (ticket == null) {
        //        return HttpNotFound();
        //    }
        //    if ((User.IsInRole("Admin") ||
        //        (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
        //        (User.IsInRole("Developer") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
        //        (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
        //        (ticket.IsArchived == false)) {
        //        return View(ticket);
        //    }
        //    return RedirectToAction("Index");
        //}

        ////POST: Tickets/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id) {
        //    Ticket ticket = db.Tickets.Find(id);
        //    // Verifying that the current user is involved with this project
        //    var project = db.Projects.First(p => p.Id == ticket.ProjectId);
        //    if  ((User.IsInRole("Admin") ||
        //        (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
        //        (User.IsInRole("Developer") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
        //        (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
        //        (ticket.IsArchived == false)) {
        //        db.Tickets.Remove(ticket);
        //        db.SaveChanges();
        //        // Now to Add the TicketHistory for the Ticket Deletion
        //        HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
        //        ticketHistory.AddHistoryEvent(ticket.Id, "DeletionDate", null, ticket.Updated.ToString());
        //        return RedirectToAction("Index");
        //    }
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
