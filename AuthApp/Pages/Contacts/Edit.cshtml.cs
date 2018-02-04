using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AuthApp.Data;
using AuthApp.Models;
using AuthApp.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace AuthApp.Pages.Contacts
{
    public class EditModel : DI_BasePageModel
    {
        private readonly AuthApp.Data.ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context,
            IAuthorizationService authorizationService,
            UserManager<ApplicationUser> userManager) : base(context, authorizationService, userManager)
        {
            _context = context;
        }

        [BindProperty]
        public Contact Contact { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Contact = await Context.Contact.FirstOrDefaultAsync(m => m.ContactId == id);

            if (id == null)
            {
                return NotFound();
            }

            if (Contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, Contact, ContactOperations.Update);

            if(!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var contact = await Context.Contact.AsNoTracking().FirstOrDefaultAsync(m => m.ContactId == id); 

            if (contact == null)
            {
                return NotFound();
            }

            var isAuthorized = await AuthorizationService.AuthorizeAsync(
                User, contact, ContactOperations.Update);

            if (!isAuthorized.Succeeded)
            {
                return new ChallengeResult();
            }


            Contact.OwnerID = contact.OwnerID;
            Context.Attach(Contact).State = EntityState.Modified;


            if(contact.Status == ContactStatus.Approved)
            {
                // if the contact is updated after approval,
                // and the user cannot approve,
                // set the status back to submitted so the update can be
                // checked and approved.
                var canApprove = await AuthorizationService.AuthorizeAsync(
                    User, contact, ContactOperations.Approve);

                if(!canApprove.Succeeded)
                {
                    contact.Status = ContactStatus.Submitted;
                }

            }

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactExists(Contact.ContactId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool ContactExists(int id)
        {
            return Context.Contact.Any(e => e.ContactId == id);
        }
    }
}
