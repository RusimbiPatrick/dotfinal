using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards.Schedules
{
    public class IndexModel : PageModel
    {
        private readonly bus_transport_mgt_sys.Models.bus_transportationDBContext _context;

        public IndexModel(bus_transport_mgt_sys.Models.bus_transportationDBContext context)
        {
            _context = context;
        }

        public IList<Schedule> Schedule { get;set; } = default!;
        public IList<string> BusPlateNosDistinct { get; set; }
        public IList<string> DepartureTimesDistinct { get; set; }

        public async Task OnGetAsync()
        {
             Schedule = await _context.Schedules
            .Include(s => s.Bus)
            .OrderByDescending(s => s.ScheduleId) // Sort by scheduleID to show most recent first
            .ToListAsync();


            BusPlateNosDistinct = Schedule.Select(s => s.Bus.PlateNo)
                                        .Where(plateNo => !string.IsNullOrEmpty(plateNo))
                                        .Distinct()
                                        .OrderBy(plateNo => plateNo)
                                        .ToList();

             DepartureTimesDistinct = Schedule
                                        .Select(s => s.DepartureTime)
                                        .Where(dt => dt != default(DateTime))
                                        .Select(dt => dt.ToString("HH:mm"))
                                        .Distinct()
                                        .OrderBy(dt => dt)
                                        .ToList();


        }
    }
}
