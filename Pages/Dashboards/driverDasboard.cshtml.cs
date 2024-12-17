using bus_transport_mgt_sys.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace bus_transport_mgt_sys.Pages.Dashboards
{
    [Authorize(Roles = "Driver")]
    public class driverDasboardModel : PageModel
    {
        private readonly ILogger<driverDasboardModel> _logger;
        private readonly bus_transportationDBContext _context;

        public driverDasboardModel(ILogger<driverDasboardModel> logger, bus_transportationDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public string UserFullName { get; private set; }
        public List<ScheduleViewModel> Schedules { get; private set; }

        public IList<string> RoutesDistinct { get; set; }
        public IList<string> BusModelsDistinct { get; set; }

        public async Task OnGetAsync()
        {
            // Retrieve user full name from session
            UserFullName = HttpContext.Session.GetString("UserFullName") ?? "Driver";

            // Retrieve userId from session or authentication
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                // If user is not logged in, redirect to login page
                _logger.LogWarning("Unauthorized access attempt to driver dashboard.");
                Response.Redirect("/Authentication/Login");
                return;
            }

            // Find the driver's assigned bus based on userId
            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.UserId == userId.Value);

            if (driver == null || string.IsNullOrEmpty(driver.AssignedBusId))
            {
                _logger.LogWarning($"Driver not found or no assigned bus for UserId: {userId}");
                Schedules = new List<ScheduleViewModel>(); // No schedules to show
                return;
            }

            // Fetch schedules only for the assigned bus
            Schedules = await _context.Schedules
                .Include(s => s.Bus)
                .ThenInclude(b => b.Route)
                .Where(s => s.Bus.PlateNo == driver.AssignedBusId) // Compare correctly
                .Select(s => new ScheduleViewModel
                {
                    RouteName = s.Bus.Route.RouteName,
                    Departure = s.Bus.Route.Departure,
                    Destination = s.Bus.Route.Destination,
                    BusPlateNo = s.Bus.PlateNo,
                    BusModel = s.Bus.Model,
                    DepartureTime = s.DepartureTime,
                    ArrivalTime = s.ArrivalTime,
                    Amount = s.Amount,
                    AvailableSeats = s.Bus.NoOfSeats - _context.Tickets.Count(t => t.ScheduleId == s.ScheduleId)
                })
                .ToListAsync();

            // Populate distinct Routes for filters
            RoutesDistinct = Schedules.Select(s => $"{s.RouteName} ({s.Departure} → {s.Destination})")
                                     .Where(route => !string.IsNullOrEmpty(route))
                                     .Distinct()
                                     .OrderBy(route => route)
                                     .ToList();

            // Populate distinct Bus Models for filters
            BusModelsDistinct = Schedules.Select(s => s.BusModel)
                                         .Where(model => !string.IsNullOrEmpty(model))
                                         .Distinct()
                                         .OrderBy(model => model)
                                         .ToList();

            // Log user access
            _logger.LogInformation($"Dashboard accessed by {UserFullName} (DriverId: {driver.DriverId}) at {DateTime.UtcNow}");
        }


        public async Task<IActionResult> OnPostLogoutAsync()
        {
            // Log logout event
            _logger.LogInformation($"User {UserFullName} logged out at {DateTime.UtcNow}");

            // Clear authentication cookie
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Clear session
            HttpContext.Session.Clear();

            // Redirect to login page
            return RedirectToPage("/Authentication/Login");
        }

        public class ScheduleViewModel
        {
            public string RouteName { get; set; }
            public string Departure { get; set; }
            public string Destination { get; set; }
            public string BusPlateNo { get; set; }
            public string BusModel { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public decimal Amount { get; set; }
            public int AvailableSeats { get; set; }
        }
    }
}
