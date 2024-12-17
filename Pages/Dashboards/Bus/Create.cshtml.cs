using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Bus
{
    public class CreateModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public CreateModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["RouteId"] = new SelectList(_context.Routes, "RouteId", "RouteName");
       
            return Page();
        }

        [BindProperty]
        public bus_transport_mgt_sys.Models.Bus Bus { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Buses.Add(Bus);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
