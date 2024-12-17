using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using bus_transport_mgt_sys.Data;
using System.Security.Claims;
using bus_transport_mgt_sys.Models;

namespace bus_transport_mgt_sys.Pages.Dashboards
{
    [Authorize(Roles = "Admin")]
    public class adminDashboardModel : PageModel
    {
        private readonly AppDbContext _appDbContext;
        private readonly bus_transportationDBContext _busDbContext;
        private readonly ILogger<adminDashboardModel> _logger;

        public adminDashboardModel(AppDbContext appDbContext, bus_transportationDBContext busDbContext, ILogger<adminDashboardModel> logger)
        {
            _appDbContext = appDbContext;
            _busDbContext = busDbContext;
            _logger = logger;
        }

        public string UserFullName { get; private set; }
        public int TotalUsers { get; private set; }
        public int ActiveBuses { get; private set; }
        public int PendingRoutes { get; private set; }
        public int OpenSupportTickets { get; private set; }
        public int ActiveUsers { get; private set; }
        public int InactiveUsers { get; private set; }



        public List<UserViewModel> RecentUsers { get; private set; }
        public List<RouteViewModel> ActiveRoutes { get; private set; }

        public void OnGet()
        {
            // Retrieve user name from claims
            UserFullName = User.FindFirst(ClaimTypes.Name)?.Value ?? "Admin";

            // Populate dashboard statistics
            ActiveBuses = _busDbContext.Buses.Count(b => b.RouteId != 0); // Buses assigned to a route
            PendingRoutes = _busDbContext.Routes.Count(r => !r.Buses.Any()); // Routes with no buses
            OpenSupportTickets = 0; // Set to 0 if no support tickets exist in this context

            // Fetch active users count
            ActiveUsers = _appDbContext.Users.Count(u => u.IsActive); // Example, adjust based on your criteria

            // Fetch recent users (last 10)
            RecentUsers = _appDbContext.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .Select(u => new UserViewModel
                {
                    FullName = u.FullName,
                    Email = u.Email,
                    RegistrationDate = u.CreatedAt
                })
                .ToList();

            // Fetch active routes
            ActiveRoutes = _busDbContext.Routes
                .Where(r => r.Buses.Any() ) // Define "active" as having buses 
                .Take(10)
                .Select(r => new RouteViewModel
                {
                    RouteName = r.RouteName,
                    StartPoint = r.Departure,
                    EndPoint = r.Destination,
                    IsActive = r.Buses.Any() 
                })
                .ToList();
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

        // View Models for dashboard data
        public class UserViewModel
        {
            public string FullName { get; set; }
            public string Email { get; set; }
            public DateTime RegistrationDate { get; set; }
        }

        public class RouteViewModel
        {
            public string RouteName { get; set; }
            public string StartPoint { get; set; }
            public string EndPoint { get; set; }
            public bool IsActive { get; set; }
        }
    }
}