using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Stops
{
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<Stop> Stop { get;set; } = default!;

        public IList<string> RouteDeparturesDistinct { get; set; }
        public IList<string> RouteDestinationsDistinct { get; set; }

        public async Task OnGetAsync()
        {
            Stop = await _context.Stops
                .Include(s => s.Route).ToListAsync();

            RouteDeparturesDistinct = _context.Routes
                                          .Select(r => r.Departure)
                                          .Where(d => !string.IsNullOrEmpty(d))
                                          .Distinct()
                                          .OrderBy(d => d)
                                          .ToList();

            RouteDestinationsDistinct = _context.Routes
                                              .Select(r => r.Destination)
                                              .Where(d => !string.IsNullOrEmpty(d))
                                              .Distinct()
                                              .OrderBy(d => d)
                                              .ToList();
        }
    }
}
