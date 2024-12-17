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
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<bus_transport_mgt_sys.Models.Bus> Bus { get;set; } = default!;
        public IList<string> ModelsDistinct { get; set; }
        public IList<string> RoutesDistinct { get; set; }



        public async Task OnGetAsync()
        {
            Bus = await _context.Buses
                .OrderByDescending(b => b.PlateNo)
                .Include(b => b.Route).ToListAsync();
                

            ModelsDistinct = Bus.Select(b => b.Model).Distinct().OrderBy(m => m).ToList();
            RoutesDistinct = Bus.Select(b => b.Route.RouteName).Distinct().OrderBy(r => r).ToList();
        }
    }
}
