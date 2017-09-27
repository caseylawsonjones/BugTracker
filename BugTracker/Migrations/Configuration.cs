namespace BugTracker.Migrations
{
    using BugTracker.Models;
    using BugTracker.Models.CodeFirst;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BugTracker.Models.ApplicationDbContext> {
        public Configuration() {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(ApplicationDbContext context) {

            // DEFAULT ROLE CREATION
            var roleManager = new RoleManager<IdentityRole>(
                new RoleStore<IdentityRole>(context));

            if (!context.Roles.Any(r => r.Name == "Admin")) {
                roleManager.Create(new IdentityRole { Name = "Admin" });
            }
            if (!context.Roles.Any(r => r.Name == "ProjectManager")) {
                roleManager.Create(new IdentityRole { Name = "ProjectManager" });
            }
            if (!context.Roles.Any(r => r.Name == "Developer")) {
                roleManager.Create(new IdentityRole { Name = "Developer" });
            }
            if (!context.Roles.Any(r => r.Name == "Submitter")) {
                roleManager.Create(new IdentityRole { Name = "Submitter" });
            }

            // DEFAULT USER CREATION
            var userManager = new UserManager<ApplicationUser>(
                new UserStore<ApplicationUser>(context));

            if (!context.Users.Any(u => u.Email == "caseylawsonjones@yahoo.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "caseylawsonjones@yahoo.com",
                    FirstName = "Casey",
                    LastName = "Jones",
                    Email = "caseylawsonjones@yahoo.com",
                }, "CoderFoundry1!");
            }
            if (!context.Users.Any(u => u.Email == "mjaang@coderfoundry.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "mjaang@coderfoundry.com",
                    FirstName = "Mark",
                    LastName = "Jaang",
                    Email = "mjaang@coderfoundry.com",
                }, "BugTracker8**");
            }
            if (!context.Users.Any(u => u.Email == "rchapman@coderfoundry.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "rchapman@coderfoundry.com",
                    FirstName = "Ryan",
                    LastName = "Chapman",
                    Email = "rchapman@coderfoundry.com",
                }, "BugTracker8**");
            }
            if (!context.Users.Any(u => u.Email == "DemoAdmin@coderfoundry.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "DemoAdmin@coderfoundry.com",
                    FirstName = "Demo",
                    LastName = "Admin",
                    Email = "DemoAdmin@coderfoundry.com",
                }, "BugTracker8**");
            }
            if (!context.Users.Any(u => u.Email == "DemoProjectManager@coderfoundry.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "DemoProjectManager@coderfoundry.com",
                    FirstName = "Demo",
                    LastName = "ProjectManager",
                    Email = "DemoProjectManager@coderfoundry.com",
                }, "BugTracker8**");
            }
            if (!context.Users.Any(u => u.Email == "DemoDeveloper@coderfoundry.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "DemoDeveloper@coderfoundry.com",
                    FirstName = "Demo",
                    LastName = "Developer",
                    Email = "DemoDeveloper@coderfoundry.com",
                }, "BugTracker8**");
            }
            if (!context.Users.Any(u => u.Email == "DemoSubmitter@coderfoundry.com")) {
                userManager.Create(new ApplicationUser {
                    UserName = "DemoSubmitter@coderfoundry.com",
                    FirstName = "Demo",
                    LastName = "Submitter",
                    Email = "DemoSubmitter@coderfoundry.com",
                }, "BugTracker8**");
            }


            // ASSIGNMENT OF ROLES TO EACH INITIAL USER

            //Casey Jones
            var userID = userManager.FindByEmail("caseylawsonjones@yahoo.com").Id;
            userManager.AddToRole(userID, "Admin");
            //Mark Jaang
            userID = userManager.FindByEmail("mjaang@coderfoundry.com").Id;
            userManager.AddToRole(userID, "Admin");
            //Ryan Chapman
            userID = userManager.FindByEmail("rchapman@coderfoundry.com").Id;
            userManager.AddToRole(userID, "Admin");

            // ASSIGNMENT OF DEMO USERS FOR EACH ROLE

            //DemoAdmin
            userID = userManager.FindByEmail("DemoAdmin@coderfoundry.com").Id;
            userManager.AddToRole(userID, "Admin");
            //DemoProjectManager
            userID = userManager.FindByEmail("DemoProjectManager@coderfoundry.com").Id;
            userManager.AddToRole(userID, "ProjectManager");
            //DemoDeveloper
            userID = userManager.FindByEmail("DemoDeveloper@coderfoundry.com").Id;
            userManager.AddToRole(userID, "Developer");
            //DemoSubmitter
            userID = userManager.FindByEmail("DemoSubmitter@coderfoundry.com").Id;
            userManager.AddToRole(userID, "Submitter");

            //TICKET STATUS SEEDS
            if (!context.TicketStatuses.Any(p => p.Name == "Submitted")) {
                var status = new TicketStatus();
                status.Name = "Submitted";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketStatuses.Any(p => p.Name == "Assigned")) {
                var status = new TicketStatus();
                status.Name = "Assigned";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketStatuses.Any(p => p.Name == "In Progress")) {
                var status = new TicketStatus();
                status.Name = "In Progress";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketStatuses.Any(p => p.Name == "Pending")) {
                var status = new TicketStatus();
                status.Name = "Pending";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketStatuses.Any(p => p.Name == "Resolved")) {
                var status = new TicketStatus();
                status.Name = "Resolved";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketStatuses.Any(p => p.Name == "Closed")) {
                var status = new TicketStatus();
                status.Name = "Closed";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketStatuses.Any(p => p.Name == "Cancelled")) {
                var status = new TicketStatus();
                status.Name = "Cancelled";
                context.TicketStatuses.Add(status);
            }

            //TICKET TYPE SEEDS
            if (!context.TicketTypes.Any(p => p.Name == "Hardware")) {
                var status = new TicketStatus();
                status.Name = "Hardware";
                context.TicketStatuses.Add(status);
            }
            if (!context.TicketTypes.Any(p => p.Name == "Software")) {
                var type = new TicketType();
                type.Name = "Software";
                context.TicketTypes.Add(type);
            }
            if (!context.TicketTypes.Any(p => p.Name == "Network")) {
                var type = new TicketType();
                type.Name = "Network";
                context.TicketTypes.Add(type);
            }

            //TICKET PRIORITY SEEDS
            if (!context.TicketPriorities.Any(p => p.Name == "Project")) {
                var priority = new TicketPriority();
                priority.Name = "Project";
                context.TicketPriorities.Add(priority);
            }
            if (!context.TicketPriorities.Any(p => p.Name == "Low")) {
                var priority = new TicketPriority();
                priority.Name = "Low";
                context.TicketPriorities.Add(priority);
            }
            if (!context.TicketPriorities.Any(p => p.Name == "Medium")) {
                var priority = new TicketPriority();
                priority.Name = "Medium";
                context.TicketPriorities.Add(priority);
            }
            if (!context.TicketPriorities.Any(p => p.Name == "High")) {
                var priority = new TicketPriority();
                priority.Name = "High";
                context.TicketPriorities.Add(priority);
            }
            if (!context.TicketPriorities.Any(p => p.Name == "Critical")) {
                var priority = new TicketPriority();
                priority.Name = "Critical";
                context.TicketPriorities.Add(priority);
            }

        }
    }
}