using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;


namespace AuthApp.Pages.Contacts
{
    public class IndexModel : DI_BasePageModel
    {
        private readonly AuthApp.Data.ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context, 
            IAuthorizationService authorizationService, 
            UserManager<ApplicationUser> userManager)
            : base(context, authorizationService, userManager)
        {
            _context = context;
        }

        public IList<Contact> Contact { get;set; }

        public async Task OnGetAsync()
        {
            var contacts = from c in Context.Contact
                           select c;

            var isAuthorized = User.IsInRole(Constants.ContactManagersRole) || User.IsInRole(Constants.ContactAdministratorsRole);

            var currentUserId = UserManager.GetUserId(User);

            // Only approved contacts are shown unless one is authorized to see them or one is the owner

            if (!isAuthorized)
            {
                contacts = contacts.Where(c =>
                c.Status == ContactStatus.Approved || c.OwnerID == currentUserId);
            }

            Contact = await contacts.ToListAsync();
        }
    }
}
