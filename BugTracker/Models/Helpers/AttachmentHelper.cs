﻿using BugTracker.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.Helpers {

    public class AttachmentHelper : Universal {

        // GET: TicketAttachments/Create
        public ActionResult Create(int? ticketId) {
            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName");
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
            if (ticketId == null || !db.Tickets.Any(t => t.Id == ticketId)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.ticketId = ticketId;
            return View();
        }

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Description,FileUrl,Created,TicketId,AuthorId")] TicketAttachment ticketAttachment, int ticketId, HttpPostedFileBase media) {
            bool noMedia = false;
            if (media != null && media.ContentLength > 0) {
                var ext = Path.GetExtension(media.FileName).ToLower(); // Gets image's extension and then sets it to lower case
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".bmp" &&
                    ext != ".doc" && ext != ".xls" && ext != ".pdf" && ext != ".txt" && ext != ".htm" &&
                    ext != ".docx" && ext != ".xlsx" && ext != ".html" && ext != ".ppt" && ext != ".pptx" &&
                    ext != ".zip" && ext != ".rar" && ext != ".jfif" && ext != ".bmp" && ext != ".bmp" &&
                    ext != ".csv" && ext != ".rtf" && ext != ".xps" && ext != ".xml" && ext != ".mp3" &&
                    ext != ".mp4" && ext != ".wav" && ext != ".avi" && ext != ".wmv" && ext != ".mov" &&
                    ext != ".flv" && ext != ".css" && ext != ".less" && ext != ".cs" && ext != ".cpp") {
                    ModelState.AddModelError("image", "Invalid Format");
                }
            }
            else {
                noMedia = true;
            }
            if (ModelState.IsValid) {
                var filePath = "Content/Media/Ticket Attachments/";
                var absPath = Server.MapPath("~/" + filePath);
                if (noMedia) {
                    return View(ticketAttachment);
                }
                ticketAttachment.FileUrl = "~/" + filePath + media.FileName;
                media.SaveAs(Path.Combine(absPath, media.FileName));
                ticketAttachment.AuthorId = User.Identity.GetUserId();
                ticketAttachment.Created = DateTimeOffset.UtcNow;
                ticketAttachment.TicketId = ticketId;
                db.TicketAttachments.Add(ticketAttachment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            return View(ticketAttachment);
        }


        // GET: TicketAttachments/Edit/5
        public ActionResult Edit(int? id) {
            if (id == null || !db.TicketComments.Any(t => t.Id == id)) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null) {
                return HttpNotFound();
            }
            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            return View(ticketAttachment);
        }


        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Description,FileUrl,Created,TicketId,AuthorId")] TicketAttachment ticketAttachment, HttpPostedFileBase media) {
            if (ModelState.IsValid) {
                db.Entry(ticketAttachment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            return View(ticketAttachment);
        }


        // GET: TicketAttachments/Delete/5
        public ActionResult Delete(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null) {
                return HttpNotFound();
            }
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id) {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            db.TicketAttachments.Remove(ticketAttachment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}