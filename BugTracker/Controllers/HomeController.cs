using BugTracker.Models;
using BugTracker.Models.CodeFirst;
using BugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers {
    [RequireHttps]
    [Authorize]
    public class HomeController : Universal {

        [Authorize]
        public ActionResult Index() {
            //return View(db.Projects.ToList());
            ProjectsListViewModel pListVM = new ProjectsListViewModel();

            // Pull a list of all Projects and Tickets into a list and then assign them to the pListVM instance of the ViewModel
            ICollection<Project> allProjects = db.Projects.ToList();
            ICollection<Ticket> allTickets = db.Tickets.ToList();
            ICollection<Project> userProjects = allProjects.Where(p => p.Users.Any(z => z.Id == User.Identity.GetUserId())).ToList();
            ICollection<Ticket> userTickets = allTickets.Where(t => t.AssignedUserId == User.Identity.GetUserId()).ToList();
            ICollection<ApplicationUser> developersWithNoTicketAssignments = db.Users.Where(u => (u.Roles.Any(r => r.RoleId == "f6c4f96d-cab2-4667-b3da-70b9165f344c")) && (u.Tickets.Count <= 0)).ToList();
            ICollection<ApplicationUser> usersWithNoRolesAssigned = db.Users.Where(u => u.Roles.Count <= 0).ToList();
            ICollection<ApplicationUser> allUsers = db.Users.ToList();
            //ICollection<ApplicationUser> developers = db.Users.Where(u => (u.Roles.Any(r => r.RoleId == "f6c4f96d-cab2-4667-b3da-70b9165f344c"))).ToList();
            pListVM.AllProjects = allProjects;
            pListVM.AllTickets = allTickets;
            pListVM.UserTickets = userTickets;
            pListVM.DevelopersWithNoTicketAssignments = developersWithNoTicketAssignments;
            pListVM.UsersWithNoRoles = usersWithNoRolesAssigned;
            pListVM.AllUsers = allUsers;
            //pListVM.Developers = developers.ToArray();

            //string[,] allProjectsTicketUserChartDataArray = new string[allProjects.Count(), 3];
            //int i = 0;
            //foreach (Project project in allProjects) {
            //    allProjectsTicketUserChartDataArray[i, 0] = project.Title;
            //    allProjectsTicketUserChartDataArray[i, 1] = project.Tickets.Count().ToString();
            //    allProjectsTicketUserChartDataArray[i, 2] = project.Users.Count().ToString();
            //    i++;
            //}
            //pListVM.AllProjectsTicketUserChartDataArray = allProjectsTicketUserChartDataArray;

            //string[,] userProjectsTicketUserChartDataArray = new string[allProjects.Count(), 3];
            //int j = 0;
            //foreach (Project userProject in userProjects) {
            //    userProjectsTicketUserChartDataArray[j, 0] = userProject.Title;
            //    userProjectsTicketUserChartDataArray[j, 1] = userProject.Tickets.Count().ToString();
            //    userProjectsTicketUserChartDataArray[j, 2] = userProject.Users.Count().ToString();
            //    i++;
            //}
            //pListVM.UserProjectsTicketUserChartDataArray = userProjectsTicketUserChartDataArray;

            //Dictionary<string, string[]> allProjectsTicketUserChartData = new Dictionary<string, string[]>();
            //string[] tempArray = new string[2];
            //foreach (Project project in allProjects) {
            //    tempArray[0] = project.Tickets.Count().ToString();
            //    tempArray[1] = project.Users.Count().ToString();
            //    allProjectsTicketUserChartData.Add(project.Title, tempArray);
            //}
            //pListVM.AllProjectsTicketUserChartData = allProjectsTicketUserChartData;
            //Dictionary<string, string[]> userProjectsTicketUserChartData = new Dictionary<string, string[]>();
            //foreach (Project userProject in userProjects) {
            //    tempArray[0] = userProject.Tickets.Count().ToString();
            //    tempArray[1] = userProject.Users.Count().ToString();
            //    userProjectsTicketUserChartData.Add(userProject.Title, tempArray);
            //}
            //pListVM.UserProjectsTicketUserChartData = userProjectsTicketUserChartData;

            ViewBag.NoRolesAssigned = false;
            if(User.IsInRole("Developer") && pListVM.UserTickets == null) {
                ViewBag.NoRolesAssigned = true;  //If user has no role assignments, this will flag true.
            }

            // I don't remember why I was pulling a list of Project Authors and cannot find where I used the information
            //List<ApplicationUser> tempUsers = new List<ApplicationUser>();
            //ApplicationUser tempUser = new ApplicationUser();
            //foreach (var project in pListVM.AllProjects) {
            //    tempUsers.Add(db.Users.FirstOrDefault(u => u.Id == project.AuthorId));
            //}
            // Admin Role does not need to be checked as all projects have already been passed to the VM.

            if (User.IsInRole("ProjectManager") || User.IsInRole("Developer") || User.IsInRole("Submitter")) {
                List<ApplicationUser> users = db.Users.ToList();
                ApplicationUser user = users.First(u => u.Id == User.Identity.GetUserId());
                pListVM.UserProjects = user.Projects.ToList();

                //foreach (var userProject in pListVM.UserProjects) {
                //    tempUser = db.Users.FirstOrDefault(u => u.Id == userProject.AuthorId);
                //    tempUsers.Add(tempUser);
                //}
            }

            //pListVM.ProjectAuthors = tempUsers;
            return View(pListVM);
        }

        //public ActionResult About() {
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact() {
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}

        //CONTACT
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Contact(EmailHelper model) {
        //    if (ModelState.IsValid) {
        //        try {
        //            var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>"; 
        //            var from = "BugTracker<webappmessages@gmail.com>";
        //            model.Body = "This is a message from your BugTracker web application.  The name and the email of the contacting person is above.";
        //                var email = new MailMessage(from,
        //                            ConfigurationManager.AppSettings["emailto"]) {  //ACHTUNG!! emailto = the program's webappmessages@gmail.com
        //                    Subject = "Portfolio Contact Email",
        //                    Body = string.Format(body, model.FromName, model.FromEmail, model.Body),
        //                    IsBodyHtml = true
        //                };
        //            var svc = new PersonalEmail();
        //            await svc.SendAsync(email);
        //            return RedirectToAction("Sent");
        //        }
        //        catch (Exception ex) {
        //            Console.WriteLine(ex.Message);
        //            await Task.FromResult(0);
        //        }
        //    }
        //    return View(model);
        //}
    }
}