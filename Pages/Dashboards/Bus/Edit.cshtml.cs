using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Bus
{
    public class EditModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public EditModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
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

            var bus =  await _context.Buses.FirstOrDefaultAsync(m => m.PlateNo == id);
            if (bus == null)
            {
                return NotFound();
            }
            Bus = bus;
           ViewData["RouteId"] = new SelectList(_context.Routes, "RouteId", "Departure");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Bus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BusExists(Bus.PlateNo))
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

        private bool BusExists(string id)
        {
            return _context.Buses.Any(e => e.PlateNo == id);
        }
    }
}
