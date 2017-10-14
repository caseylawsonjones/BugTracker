using BugTracker.Models.CodeFirst;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.Helpers {
    [Authorize]
    public class ProjectAssignHelper {

        private ApplicationDbContext db = new ApplicationDbContext();

        // IsUseronProject
        public bool IsUseronProject(string userId, int projectId) {
            var project = db.Projects.Find(projectId);
            var userBool = project.Users.Any(u => u.Id == userId);
            return (userBool);
        }

        // AddUserToProject
        [Authorize(Roles = "Admin, ProjectManager")]
        public void AddUserToProject(string userId, int projectId) {
            var user = db.Users.Find(userId);
            var project = db.Projects.Find(projectId);
            project.Users.Add(user);
            db.SaveChanges();
        }

        // RemoveUserFromProject
        [Authorize(Roles = "Admin, ProjectManager")]
        public void RemoveUserFromProject(string userId, int projectId) {
            var user = db.Users.Find(userId);
            var project = db.Projects.Find(projectId);
            project.Users.Remove(user);
            db.SaveChanges();
        }

        // ListUserProjects
        public ICollection<Project> ListUserProjects(string userId, int projectId) {
            var user = db.Users.Find(userId);
            return user.Projects.ToList();
        }

        //ListUsersOnProject
        public ICollection<ApplicationUser> ListUsersOnProject(int projectId) {
            var project = db.Projects.Find(projectId);
            return project.Users.ToList();
        }

        //ListUsersNotOnProject
        public ICollection<ApplicationUser> ListUsersNotOnProject(int projectId) {
            return db.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }


    }
}