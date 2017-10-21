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
using System.IO;
using BugTracker.Models.Helpers;
using System.Threading.Tasks;

namespace BugTracker.Controllers {
    [RequireHttps]
    public class TicketAttachmentsController : Universal {

        // GET: TicketAttachments
        //public ActionResult Index() {
        //    var ticketAttachments = db.TicketAttachments.Include(t => t.Author).Include(t => t.Ticket);
        //    return View(ticketAttachments.ToList());
        //}

        //// GET: TicketAttachments/Details/5
        //public ActionResult Details(int? id) {
        //    if (id == null) {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
        //    if (ticketAttachment == null) {
        //        return HttpNotFound();
        //    }
        //    return View(ticketAttachment);
        //}

        // GET: TicketAttachments/Create

        public ActionResult Create(int ticketId) {
            if (!db.Tickets.Any(t => t.Id == ticketId)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.First(t => t.Id == ticketId);
            Project project = db.Projects.First(p => p.Tickets.Any(x => x.Id == ticketId));
            if  ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                ViewBag.ticketId = ticketId;
                return View();
            }
            return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Description,FileUrl,Created,TicketId,AuthorId")] TicketAttachment ticketAttachment, int ticketId, ICollection<HttpPostedFileBase> files) {
            if (!db.Tickets.Any(t => t.Id == ticketId)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.First(t => t.Id == ticketId);
            Project project = db.Projects.First(p => p.Tickets.Any(x => x.Id == ticketId));
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                if (ModelState.IsValid) {
                    TicketAttachment attachment = new TicketAttachment();
                    List<string> attachments = new List<string>();
                    foreach (var att in files) {
                        if (att != null && att.ContentLength > 0) {

                            // Process File Info and Save File
                            var ext = Path.GetExtension(att.FileName).ToLower(); // Gets image's extension and then sets it to lower case
                            var filePath = "Content/Media/Ticket Attachments/" + ticketId;
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
                            attachments.Add(newFileName);
                            att.SaveAs(Path.Combine(absPath, newFileName));

                            // Update attachment info and add to database
                            attachment.FileUrl = "/" + filePath + "/" + newFileName;
                            attachment.FileName = newFileName;
                            attachment.AuthorId = User.Identity.GetUserId();
                            attachment.Created = DateTime.UtcNow;
                            attachment.TicketId = ticketId;
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

                            // Now add the TicketHistory for the TicketAttachment Creation
                            HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
                            ticketHistory.AddHistoryEvent(ticketAttachment.TicketId, "Attachment Added", null, attachment.FileUrl);
                        }
                    }

                    // Now, send notification, if Ticket.AssignedUserId == Current User
                    if (ticket.AssignedUserId != null && User.Identity.GetUserId() != ticket.AssignedUserId) {
                        ApplicationUser assignedUser = db.Users.Find(ticket.AssignedUserId);
                        string assignedUserEmailAddress = assignedUser.Email;
                        bool attAdded = true;
                        NotificationHelper notification = new NotificationHelper(User.Identity.GetUserId());
                        await notification.AddAttachmentNotification(ticket.Id, attachments, attAdded, assignedUserEmailAddress);
                    }
                    return RedirectToAction("Details", "Tickets", new { id = ticketId });
                }

                //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
                //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
                return View(ticketAttachment);
            }
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        // GET: TicketAttachments/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null || !db.TicketComments.Any(t => t.Id == id)) {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
        //    if (ticketAttachment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
        //    //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
        //    return View(ticketAttachment);
        //}

        //// POST: TicketAttachments/Edit/5
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "Id,Description,FileUrl,Created,TicketId,AuthorId")] TicketAttachment ticketAttachment, HttpPostedFileBase media)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(ticketAttachment).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
        //    //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
        //    return View(ticketAttachment);
        //}

        // GET: TicketAttachments/Delete/5

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment attachment = db.TicketAttachments.Find(id);
            Ticket ticket = db.Tickets.First(t => t.Id == attachment.TicketId);
            Project project = db.Projects.Find(ticket.ProjectId);
            if  ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
                if (ticketAttachment == null) {
                    return HttpNotFound();
                }
                return View(ticketAttachment);
            }
            return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            Ticket ticket = db.Tickets.First(t => t.Id == ticketAttachment.TicketId);
            Project project = db.Projects.Find(ticket.ProjectId);
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                // Now delete from database
                db.TicketAttachments.Remove(ticketAttachment);
                db.SaveChanges();

                // Now delete the file from the repository
                var absPath = Server.MapPath(ticketAttachment.FileUrl);
                //att.SaveAs(Path.Combine(absPath, att.FileName));
                try {
                    System.IO.File.Delete(@absPath);
                }
                catch (System.IO.IOException e) {
                    Console.WriteLine(e.Message);
                    Console.ReadLine();
                    return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
                }

                // Now add the TicketHistory for the TicketAttachment Deletion
                HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
                ticketHistory.AddHistoryEvent(ticketAttachment.TicketId, "Attachment Deleted", ticketAttachment.FileUrl, null);

                // Now send notification, if assignedUser != Current User
                if (ticket.AssignedUserId != null && ticket.AssignedUserId != User.Identity.GetUserId()) {
                    List<string> attachments = new List<string>();
                    attachments.Add(ticketAttachment.FileName);
                    string assignedUserEmailAddress = db.Users.Find(ticket.AssignedUserId).Email;
                    bool attAdded = false;
                    NotificationHelper notification = new NotificationHelper(User.Identity.GetUserId());
                    await notification.AddAttachmentNotification(ticket.Id, attachments, attAdded, assignedUserEmailAddress);
                }

                return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
            }
            return RedirectToAction("Details", "Tickets", new { id = ticket.Id });
        }

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
