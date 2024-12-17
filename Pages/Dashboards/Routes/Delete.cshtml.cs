using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Routes
{
    public class DeleteModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public DeleteModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public bus_transport_mgt_sys.Models.Route Route { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes.FirstOrDefaultAsync(m => m.RouteId == id);

            if (route == null)
            {
                return NotFound();
            }
            else
            {
                Route = route;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes.FindAsync(id);
            if (route != null)
            {
                Route = route;
                _context.Routes.Remove(Route);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
