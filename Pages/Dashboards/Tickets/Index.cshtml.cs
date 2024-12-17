using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Tickets
{
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<Ticket> Ticket { get;set; } = default!;
        public IList<string> BusPlateNosDistinct { get; set; }
        public IList<string> ScheduleBusIdsDistinct { get; set; }
        public IList<string> UserEmailsDistinct { get; set; }

        public async Task OnGetAsync()
        {
            Ticket = await _context.Tickets
                .Include(t => t.Bus)
                .Include(t => t.Schedule)
                .Include(t => t.User).ToListAsync();

            BusPlateNosDistinct = Ticket.Select(t => t.Bus.PlateNo)
                                        .Where(plateNo => !string.IsNullOrEmpty(plateNo))
                                        .Distinct()
                                        .OrderBy(plateNo => plateNo)
                                        .ToList();

            // Populate distinct Schedule Bus IDs for filters
            ScheduleBusIdsDistinct = Ticket.Select(t => t.Schedule.BusId)
                                          .Distinct()
                                          .OrderBy(busId => busId)
                                          .ToList();

            // Populate distinct User Emails for filters
            UserEmailsDistinct = Ticket.Select(t => t.User.Email)
                                      .Where(email => !string.IsNullOrEmpty(email))
                                      .Distinct()
                                      .OrderBy(email => email)
                                      .ToList();
        }
    }
}
