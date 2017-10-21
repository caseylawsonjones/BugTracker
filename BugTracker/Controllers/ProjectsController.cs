using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using BugTracker.Models.CodeFirst;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize]
    public class ProjectsController : Universal
    {

        //// GET: Projects
        //[Authorize]
        //public ActionResult Index()
        //{
        //    //return View(db.Projects.ToList());
        //    ProjectsListViewModel pListVM = new ProjectsListViewModel();
        //    ICollection<Project> projects = db.Projects.ToList();
        //    pListVM.AllProjects = projects;
        //    ViewBag.NoRolesAssigned = false;
        //    ICollection<ApplicationUser> unassignedUsers = db.Users.Where(u => u.Tickets.Count <= 0).ToList();
        //    pListVM.UsersWithNoAssignments = unassignedUsers;
        //    ICollection<ApplicationUser> usersWithNoRolesAssigned = db.Users.Where(u => u.Roles.Count <= 0).ToList();
        //    pListVM.UsersWithNoRoles = usersWithNoRolesAssigned;

        //    List<ApplicationUser> tempUsers = new List<ApplicationUser>();
        //    ApplicationUser tempUser = new ApplicationUser();
        //    foreach (var project in pListVM.AllProjects) {
        //        tempUsers.Add(db.Users.FirstOrDefault(u => u.Id == project.AuthorId));
        //    }
        //    // Admin Role does not need to be checked as all projects have already been passed to the VM.

        //    if (User.IsInRole("ProjectManager") || User.IsInRole("Developer") || User.IsInRole("Submitter")) {
        //        List <ApplicationUser> users = db.Users.ToList();
        //        ApplicationUser user = users.First(u => u.Id == User.Identity.GetUserId());
        //        pListVM.UserProjects = user.Projects.ToList();
        //        foreach (var userProject in pListVM.UserProjects) {
        //            tempUser = db.Users.FirstOrDefault(u => u.Id == userProject.AuthorId);
        //            tempUsers.Add(tempUser);
        //        }
        //    }
        //    if (pListVM == null) {
        //        ViewBag.NoRolesAssigned = true;  //If user has no role assignments, this will flag true.
        //    }

        //    pListVM.ProjectAuthors = tempUsers;
        //    return View(pListVM);
        //}

        //public ActionResult ProjectList() {
        //    List<Project> mine = new List<Project>();
        //    List<Project> projList = db.Projects.ToList();
        //    foreach(var project in projList) {
        //        foreach(var user in project.Users) {
        //            if (user.Id == User.Identity.GetUserId()) {
        //                mine.Add(project);
        //            }
        //        }
        //    }
        //    return View("ProjectList", mine);
        //}


        // GET: Projects/Details/5

        public ActionResult Details(int? id)
        {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var project = db.Projects.Find(id);
            if (project == null) {
                return HttpNotFound();
            }

            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager") || project.Users.Any(u => u.Id == User.Identity.GetUserId())) {
                ProjectUserViewModel projectUserVM = new ProjectUserViewModel();
                projectUserVM.Project = project;
                projectUserVM.ProjectId = project.Id;
                projectUserVM.ProjectOwner = db.Users.Find(project.AuthorId).FullName;
                projectUserVM.AssignedUsersList = project.Users.ToList();
                //projectUserVM.AssignedUsers = project.Users.Select(u => u.Id).ToArray();
                //Sends the assigned Users' Ids to an array
                //projectUserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "FullName", projectUserVM.AssignedUsers);
                // Verifying that the current user is involved with this project
                return View(projectUserVM);
            }
            else {
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create([Bind(Include = "Id,Title,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Created = DateTimeOffset.UtcNow;
                project.AuthorId = User.Identity.GetUserId();
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Updated = DateTimeOffset.UtcNow;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        //[Authorize(Roles = "Admin, ProjectManager")]
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    Project project = db.Projects.Find(id);
        //    if (project == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(project);
        //}

        // POST: Projects/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //[Authorize(Roles = "Admin, ProjectManager")]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Project project = db.Projects.Find(id);
        //    db.Projects.Remove(project);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        // GET:

        [Authorize (Roles = "Admin, ProjectManager")]
        public ActionResult ProjectUser(int id) {

            var project = db.Projects.Find(id);
            ProjectUserViewModel projectUserVM = new ProjectUserViewModel();
            projectUserVM.Project = project;
            projectUserVM.ProjectId = project.Id;
            projectUserVM.AssignedUsers = project.Users.Select(u => u.Id).ToArray();  //Sends the assigned Users' Ids to an array
            projectUserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "FullName", projectUserVM.AssignedUsers);
            return View(projectUserVM);
        }

        //POST:
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult ProjectUser(ProjectUserViewModel model) {

            ProjectAssignHelper helper = new ProjectAssignHelper();
            foreach (var userId in db.Users.Select(r => r.Id).ToList()) {
                helper.RemoveUserFromProject(userId, model.ProjectId);
            }
            foreach(var userId in model.AssignedUsers) {
                helper.AddUserToProject(userId, model.ProjectId);
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Archive(int? id) {
            if (id == null) {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null) {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Archive/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Archive([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId")] Project project) {
            if (ModelState.IsValid) {
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Home");
            }
            return View(project);
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
