using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Drivers
{
    public class DetailsModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public DetailsModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public Driver Driver { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers.FirstOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }
            else
            {
                Driver = driver;
            }
            return Page();
        }
    }
}
