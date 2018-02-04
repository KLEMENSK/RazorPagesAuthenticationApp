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
    public class DetailsModel : DI_BasePageModel
    {
        private readonly AuthApp.Data.ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context, 
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager) : base(context, authorizationService, userManager)
        {
            _context = context;
        }

        public Contact Contact { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            Contact = await Context.Contact.FirstOrDefaultAsync(m => m.ContactId == id);

            if (Contact == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id, ContactStatus status)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contact = await Context.Contact.FirstOrDefaultAsync(m => m.ContactId == id);

            if (contact == null)
            {
                return NotFound();
            }

            var contactOperation = (status == ContactStatus.Approved)
                                            ? ContactOperations.Approve
                                            : ContactOperations.Reject;

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, contact, contactOperation);

            if(!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }
            contact.Status = status;
            Context.Contact.Update(contact);
            await Context.SaveChangesAsync();

            return RedirectToPage("./Index");

        }
    }
}
