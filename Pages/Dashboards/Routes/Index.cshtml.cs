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
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<bus_transport_mgt_sys.Models.Route> Route { get;set; } = default!;
        public IList<string> DeparturesDistinct { get; set; }
        public IList<string> DestinationsDistinct { get; set; }

        public async Task OnGetAsync()
        {
            Route = await _context.Routes.ToListAsync();

            DeparturesDistinct = Route.Select(r => r.Departure)
                                   .Where(d => !string.IsNullOrEmpty(d))
                                   .Distinct()
                                   .OrderBy(d => d)
                                   .ToList();

            DestinationsDistinct = Route.Select(r => r.Destination)
                                       .Where(d => !string.IsNullOrEmpty(d))
                                       .Distinct()
                                       .OrderBy(d => d)
                                       .ToList();
        }
    }
}
