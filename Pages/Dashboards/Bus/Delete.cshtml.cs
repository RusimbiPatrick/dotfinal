using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Bus
{
    public class DeleteModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public DeleteModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public bus_transport_mgt_sys.Models.Bus Bus { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Buses.FirstOrDefaultAsync(m => m.PlateNo == id);

            if (bus == null)
            {
                return NotFound();
            }
            else
            {
                Bus = bus;
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bus = await _context.Buses.FindAsync(id);
            if (bus != null)
            {
                Bus = bus;
                _context.Buses.Remove(Bus);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
