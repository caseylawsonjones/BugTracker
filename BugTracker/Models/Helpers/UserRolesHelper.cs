using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models.Helpers {
    [Authorize (Roles = "Admin")]
    public class UserRolesHelper : Universal {

        private UserManager<ApplicationUser> userManager =
            new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(new ApplicationDbContext()));

        // IsUserInRole
        public bool IsUserInRole(string userId, string roleName) {
            return userManager.IsInRole(userId, roleName);
        }

        // ListUserRoles
        public ICollection<string> ListUserRoles(string userId) {
            return userManager.GetRoles(userId);
        }

        // AddUserToRole
        [Authorize(Roles = "Admin")]
        public bool AddUserToRole(string userId, string roleName) {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        // RemoveUserFromRole
        [Authorize(Roles = "Admin")]
        public bool RemoveUserFromRole(string userId, string roleName) {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        // UsersInRole
        public ICollection<ApplicationUser> UsersInRole(string roleName) {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = userManager.Users.ToList();
            foreach(var user in List) {
                if(IsUserInRole(user.Id, roleName)) {
                    resultList.Add(user);
                }
            }
            return resultList;
        }

        // UsersNotInRole
        public ICollection<ApplicationUser> UsersNotInRole(string roleName) {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = userManager.Users.ToList();
            foreach (var user in List) {
                if (!IsUserInRole(user.Id, roleName)) {
                    resultList.Add(user);
                }
            }
            return resultList;
        }







    }
}