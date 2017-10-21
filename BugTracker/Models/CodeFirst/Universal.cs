using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using System.Data.Entity;

namespace BugTracker.Models {
    public class Universal : Controller {
        public ApplicationDbContext db = new ApplicationDbContext();
        protected override void OnActionExecuting(ActionExecutingContext filterContext) {
            if (User.Identity.IsAuthenticated) {
                var userId = User.Identity.GetUserId();
                ApplicationUser user = db.Users.AsNoTracking().FirstOrDefault(u => u.Id == userId);
                ViewBag.FirstName = user.FirstName;
                ViewBag.LastName  = user.LastName;
                ViewBag.FullName  = user.FullName;
                ViewBag.UserTimeZone = db.Users.Find(User.Identity.GetUserId()).TimeZone;

            }
            base.OnActionExecuting(filterContext); //This needs to be here
        }
    }
}