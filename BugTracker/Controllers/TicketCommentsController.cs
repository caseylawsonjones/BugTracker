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
using System.Threading.Tasks;

namespace BugTracker.Controllers
{
    [Authorize]
    public class TicketCommentsController : Universal
    {

        // GET: TicketComments
        //public ActionResult Index()
        //{
        //    var ticketComments = db.TicketComments.Include(t => t.Ticket);
        //    return View(ticketComments.ToList());
        //}

        // GET: TicketComments/Details/5

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            var project = db.Projects.First(u => u.Tickets.Any(t => t.Id == ticketComment.TicketId));
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            if (User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticketComment.AuthorId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticketComment.AuthorId == User.Identity.GetUserId())) {
                return View(ticketComment);
            }
            return RedirectToAction("Index", "TicketComments");
        }

        // GET: TicketComments/Create
        public ActionResult Create(int id)
        {
            TicketComment comment = db.TicketComments.Find(id);
            Ticket ticket = db.Tickets.Find(comment.TicketId);
            Project project = db.Projects.Find(ticket.ProjectId);
            if  ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                ViewBag.ticketId = id;
                return View();
            }
            return RedirectToAction("Index", "TicketComments");
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Body")] TicketComment ticketComment, int ticketId)
        {
            TicketComment comment = db.TicketComments.Find(id);
            Ticket ticket = db.Tickets.Find(comment.TicketId);
            Project project = db.Projects.Find(ticket.ProjectId);
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticket.AssignedUserId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticket.OwnerUserId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                if (ModelState.IsValid) {
                    // Now add the TicketHistory for the TicketComment Creation
                    HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
                    ticketHistory.AddHistoryEvent(ticket.Id, "Comment Created", null, ticketComment.Body);
                    ticketComment.Created = DateTimeOffset.UtcNow;
                    ticketComment.TicketId = ticketId;
                    ticketComment.AuthorId = User.Identity.GetUserId();
                    db.TicketComments.Add(ticketComment);
                    db.SaveChanges();

                    // Now send Notification, if ticket.AssignedUserId != Current User
                    if (User.Identity.GetUserId() != ticket.AssignedUserId) {
                        string assignedUserEmailAddress = db.Users.Find(ticket.AssignedUserId).Email;
                        bool commentAdded = true;
                        NotificationHelper notification = new NotificationHelper(User.Identity.GetUserId());
                        await notification.AddCommentNotification(ticket.Id, ticketComment.Body, commentAdded, assignedUserEmailAddress);
                    }

                    return RedirectToAction("Details", "Tickets", new { id = ticketId });
                }
                ViewBag.ticketId = ticketId;
                return View(ticketComment);
                //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            }
        }

        // GET: TicketComments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null || !db.TicketComments.Any(c => c.Id == id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            Ticket ticket = db.Tickets.Find(ticketComment.TicketId);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            Project project = db.Projects.First(p => p.Id == ticket.ProjectId);
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticketComment.AuthorId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticketComment.AuthorId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                return View(ticketComment);
            }
            return RedirectToAction("Index", "TicketComments");
            // Original Scaffolded Code included this
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Body,Created,TicketId,AuthorId")] TicketComment ticketComment)
        {
            Ticket ticket = db.Tickets.Find(ticketComment.TicketId);
            Project project = db.Projects.First(p => p.Id == ticket.ProjectId);
            if ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticketComment.AuthorId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticketComment.AuthorId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                if (ModelState.IsValid) {
                    // Now add the TicketHistory for the TicketComment Edit
                    TicketComment oldComment = db.TicketComments.AsNoTracking().First(t => t.Id == ticketComment.Id);
                    HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
                    ticketHistory.AddHistoryEvent(ticketComment.TicketId, "Comment Edited", oldComment.Body, ticketComment.Body);

                    db.Entry(ticketComment).State = EntityState.Modified;
                    ticketComment.Updated = DateTime.Now;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
                return View(ticketComment);
            }
            return RedirectToAction("Index");
        }

        // GET: TicketComments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            Ticket ticket = db.Tickets.Find(ticketComment.TicketId);
            if (ticketComment == null) {
                return HttpNotFound();
            }
            var project = db.Projects.First(p => p.Tickets.Any(t => t.Id == ticketComment.TicketId));
            if  ((User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticketComment.AuthorId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticketComment.AuthorId == User.Identity.GetUserId())) &&
                (ticket.IsArchived == false)) {
                return View(ticketComment);
            }
            return RedirectToAction("Index", "TicketComments");
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            // Now add the TicketHistory for the TicketComment Deletion
            TicketComment ticketComment = db.TicketComments.Find(id);
            HistoryHelper ticketHistory = new HistoryHelper(User.Identity.GetUserId());
            ticketHistory.AddHistoryEvent(ticketComment.TicketId, "Comment Deleted", null, null);
            int ticketId = ticketComment.TicketId;
            var project = db.Projects.First(p => p.Tickets.Any(t => t.Id == ticketComment.TicketId));
            if (User.IsInRole("Admin") ||
                (User.IsInRole("ProjectManager") && project.Users.Any(u => u.Id == User.Identity.GetUserId())) ||
                (User.IsInRole("Developer") && ticketComment.AuthorId == User.Identity.GetUserId()) ||
                (User.IsInRole("Submitter") && ticketComment.AuthorId == User.Identity.GetUserId())) {
                db.TicketComments.Remove(ticketComment);
                db.SaveChanges();

                // Now send Notification, if ticket.AssignedUserId != Current User
                Ticket ticket = db.Tickets.Find(ticketId);
                if (User.Identity.GetUserId() != ticket.AssignedUserId) {
                    string assignedUserEmailAddress = db.Users.Find(ticket.AssignedUserId).Email;
                    bool commentAdded = false;
                    NotificationHelper notification = new NotificationHelper(User.Identity.GetUserId());
                    await notification.AddCommentNotification(ticket.Id, ticketComment.Body, commentAdded, assignedUserEmailAddress);
                }


                return RedirectToAction("Details", "Tickets", new { id = ticketId });
            }
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
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
