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
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<string> LicencesDistinct { get; set; }
        public IList<string> AssignedBusesDistinct { get; set; }
        public IList<Driver> Driver { get;set; } = default!;

        public async Task OnGetAsync()
        {
            Driver = await _context.Drivers
                .OrderByDescending(d => d.DriverId)
                .Include(d => d.AssignedBus).ToListAsync();

            LicencesDistinct = Driver.Select(d => d.Licence).Distinct().OrderBy(l => l).ToList();
            AssignedBusesDistinct = Driver.Select(d => d.AssignedBus.PlateNo).Distinct().OrderBy(b => b).ToList();
        }
    }
}
