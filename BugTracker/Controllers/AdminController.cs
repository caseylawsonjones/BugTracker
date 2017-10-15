using BugTracker.Models;
using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public class AdminController : Universal
    {
        private UserRolesHelper helper = new UserRolesHelper();

        // GET: Admin
        public ActionResult Index() {
            List<AdminUserViewModels> users = new List<AdminUserViewModels>();
            UserRolesHelper helper = new UserRolesHelper();
            foreach (var user in db.Users.ToList()) {
                var eachUser = new AdminUserViewModels();
                eachUser.User = user;
                eachUser.AssignedRoles = helper.ListUserRoles(user.Id).ToArray();
                users.Add(eachUser);
            }
            return View(users.OrderBy(u => u.User.LastName).ToList());
        }

        //GET: EditUserRoles
        public ActionResult EditUserRoles(string id) {
            var user = db.Users.Find(id);
            var helper = new UserRolesHelper();
            var model = new AdminUserViewModels();
            model.User = user;
            model.AssignedRoles = helper.ListUserRoles(id).ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.AssignedRoles);
            return View(model);
        }

        //POST: EditUserRoles
        [HttpPost]
        public ActionResult EditUserRoles(AdminUserViewModels model) {
            var user = db.Users.Find(model.User.Id);
            UserRolesHelper helper = new UserRolesHelper();

            foreach (var role in db.Roles.Select(r => r.Name).ToList()) {
                helper.RemoveUserFromRole(user.Id, role);
            }

            if (model.AssignedRoles != null) {
                foreach (var role in model.AssignedRoles) {
                    helper.AddUserToRole(user.Id, role);
                }

                return RedirectToAction("Index");
            }
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