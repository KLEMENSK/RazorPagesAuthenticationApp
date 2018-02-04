using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using AuthApp.Data;
using Microsoft.AspNetCore.Identity;
using AuthApp.Authorization;

namespace AuthApp.Models
{
    public class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {

                using (var context = new ApplicationDbContext(
                    serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
                {

                    // For test purposes we are seeding 2 users both with the same password.
                    // The password is set with the following command:
                    // dotnet user-secrets set SeedUserPW <pw>
                    // The admin user can do anything
                    var adminID = await EnsureUser(serviceProvider, testUserPw, "noreply.cloudenergy@gmail.com");
                    await EnsureRole(serviceProvider, adminID, Constants.ContactAdministratorsRole);

                // allowed user can create and edit contacts that they create
                var uid = await EnsureUser(serviceProvider, testUserPw, "noreply.cloudenergy@gmail.com");
                await EnsureRole(serviceProvider, uid, Constants.ContactManagersRole);

                    if (context.Contact.Any())
                    {
                        // Deletion of entries in case they exist
                        List<Contact> remCont = context.Contact.Where(d => d.Name != "").ToList();
                        context.Contact.RemoveRange(remCont);
                        // if no deletion is desired for the database, such as in a real production environment, then use
                        // the following code
                        //return;   // DB has been seeded
                    }

                    context.Contact.AddRange(
                        new Contact
                        {
                            Name = "Debra Garcia",
                            Address = "1234 Main St",
                            City = "Redmond",
                            State = "WA",
                            Zip = "10999",
                            Email = "debra@example.com",
                            Status = ContactStatus.Approved,
                            OwnerID = adminID
                        },
                        new Contact
                        {
                            Name = "Thorsten Weinrich",
                            Address = "5678 1st Ave W",
                            City = "Redmond",
                            State = "WA",
                            Zip = "10999",
                            Email = "thorsten@example.com",
                            Status = ContactStatus.Submitted,
                            OwnerID = "klemens.cloudenergy@gmail.com"
                        },
                        new Contact
                        {
                            Name = "Yuhong Li",
                            Address = "9012 State st",
                            City = "Redmond",
                            State = "WA",
                            Zip = "10999",
                            Email = "yuhong@example.com",
                            Status = ContactStatus.Rejected,
                            OwnerID = "klemens.cloudenergy@gmail.com"
                        },
                        new Contact
                        {
                            Name = "Jon Orton",
                            Address = "3456 Maple St",
                            City = "Redmond",
                            State = "WA",
                            Zip = "10999",
                            Email = "jon@example.com",
                            Status = ContactStatus.Submitted,
                            OwnerID = "klemens.cloudenergy@gmail.com"
                        },
                        new Contact
                        {
                            Name = "Diliana Alexieva-Bosseva",
                            Address = "7890 2nd Ave E",
                            City = "Redmond",
                            State = "WA",
                            Zip = "10999",
                            Email = "diliana@example.com",
                            Status = ContactStatus.Submitted,
                            OwnerID = "klemens.cloudenergy@gmail.com"
                        }
                        );
                    context.SaveChanges();
                }

        }

        
        private static async Task<string> EnsureUser(IServiceProvider serviceProvider, 
            string testUserPw, string UserName)
        {
            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();

            var user = await userManager.FindByNameAsync(UserName);
            if ( user == null)
            {
                user = new ApplicationUser { UserName = UserName };
                await userManager.CreateAsync(user, testUserPw);
            }

            return user.Id;
        }

        private static async Task<IdentityResult> EnsureRole(IServiceProvider serviceProvider, 
            string uid, string role)
        {
            IdentityResult IR = null;
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(role))
            {
                IR = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(uid);
            IR = await userManager.AddToRoleAsync(user, role);

            return IR;
        }

    }
}
