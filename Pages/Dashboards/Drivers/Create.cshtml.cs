using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using bus_transport_mgt_sys.Models;
using bus_transport_mgt_sys.Services;
using bus_transport_mgt_sys.Interfaces; // Make sure to add this

namespace bus_transport_mgt_sys.Pages.Dashboards.Drivers
{
    public class CreateModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;
        private readonly IEmailService _emailService; // Add this field

        // Update constructor to include IEmailService
        public CreateModel(
            bus_transport_mgt_sys.Models.bus_transportationDBContext context,
            IEmailService emailService) // Add this parameter
        {
            _context = context;
            _emailService = emailService; // Initialize the email service
        }

        public IActionResult OnGet()
        {
            ViewData["AssignedBusId"] = new SelectList(_context.Buses, "PlateNo", "PlateNo");
            ViewData["UserId"] = new SelectList(_context.Users, "UserId", "Email");
            return Page();
        }

        [BindProperty]
        public Driver Driver { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Drivers.Add(Driver);

            // Update the role of the selected user to "Driver"
            var user = await _context.Users.FindAsync(Driver.UserId);
            if (user != null)
            {
                user.Role = "Driver";
                _context.Users.Update(user);

                // Send email notification using the injected service
                await _emailService.SendEmailAsync(
                    user.Email,
                    "Driver Role Assignment",
                    $"Dear {user.FullName},\n\nYou have been assigned the role of Driver in our transportation management system."
                );
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}