using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using bus_transport_mgt_sys.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using bus_transport_mgt_sys.Interfaces;

namespace bus_transport_mgt_sys.Pages.Dashboards
{
    public class clientDashboardModel : PageModel
    {
        private readonly ILogger<clientDashboardModel> _logger;
        private readonly bus_transportationDBContext _context;
        private readonly IEmailService _emailService;

        public clientDashboardModel(
            ILogger<clientDashboardModel> logger,
            bus_transportationDBContext context,
            IEmailService emailService) // Add IEmailService
        {
            _logger = logger;
            _context = context;
            _emailService = emailService; // Initialize email service
        }

        public string UserFullName { get; private set; }
        public List<ScheduleViewModel> Schedules { get; private set; }
        public IList<string> RoutesDistinct { get; set; }
        public IList<string> BusModelsDistinct { get; set; }

        public async Task OnGetAsync()
        {
            // Retrieve user full name from session
            UserFullName = HttpContext.Session.GetString("UserFullName") ?? "User";


            // Fetch schedules with related data
            Schedules = await _context.Schedules
                .Include(s => s.Bus)
                .ThenInclude(b => b.Route)
                .Select(s => new ScheduleViewModel
                {
                    ScheduleId = s.ScheduleId,
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
                .OrderByDescending(s => s.ScheduleId)
                .ToListAsync();

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
            _logger.LogInformation($"Dashboard accessed by {UserFullName} at {DateTime.UtcNow}");
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

        public async Task<IActionResult> OnPostBookTicketAsync([FromForm] BookTicketInput input)
        {
            try
            {
                // Validate input schedule
                var schedule = await _context.Schedules
                    .Include(s => s.Bus)
                        .ThenInclude(b => b.Route)
                    .FirstOrDefaultAsync(s => s.ScheduleId == input.ScheduleId);

                if (schedule == null)
                {
                    TempData["ErrorMessage"] = "Schedule not found.";
                    return Page();
                }

                // Validate the entered amount
                if (input.Amount != schedule.Amount)
                {
                    TempData["ErrorMessage"] = "Invalid amount entered.";
                    return Page();
                }

                // Validate the phone number format (10 digits)
                var phoneRegex = new Regex(@"^[0-9]{10}$");
                if (!phoneRegex.IsMatch(input.PhoneNumber))
                {
                    TempData["ErrorMessage"] = "Invalid phone number format. Must be 10 digits.";
                    return Page();
                }

                // Check seat availability
                int bookedSeats = await _context.Tickets.CountAsync(t => t.ScheduleId == input.ScheduleId);
                if (bookedSeats >= schedule.Bus.NoOfSeats)
                {
                    TempData["ErrorMessage"] = "No seats available for this schedule.";
                    return Page();
                }

                // Retrieve user
                var userId = HttpContext.Session.GetInt32("UserId");
                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "User session expired. Please log in again.";
                    return RedirectToPage("/Login");
                }

                var user = await _context.Users.FindAsync(userId.Value);
                if (user == null)
                {
                    TempData["ErrorMessage"] = "User not found.";
                    return Page();
                }

                // Validate critical user information
                if (string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.FullName))
                {
                    TempData["ErrorMessage"] = "Incomplete user profile. Please update your profile.";
                    return RedirectToPage("/Profile/Edit");
                }

                // Book the ticket
                var ticket = new Ticket
                {
                    BusId = schedule.BusId,
                    ScheduleId = input.ScheduleId,
                    UserId = userId.Value
                };

                // Ensure database operations are within a transaction
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // Add ticket
                        _context.Tickets.Add(ticket);
                        await _context.SaveChangesAsync();

                        // Store payment details
                        var payment = new Payment
                        {
                            TicketId = ticket.TicketId,
                            Amount = input.Amount.ToString(),
                            PaymentDate = DateTime.UtcNow,
                            PaymentMethod = input.PaymentMethod
                        };

                        _context.Payments.Add(payment);
                        await _context.SaveChangesAsync();

                        // Commit transaction
                        await transaction.CommitAsync();
                    }
                    catch (Exception transactionEx)
                    {
                        // Rollback transaction on failure
                        await transaction.RollbackAsync();
                        _logger.LogError($"Transaction failed: {transactionEx.Message}");
                        TempData["ErrorMessage"] = "Failed to complete booking. Please try again.";
                        return Page();
                    }
                }

                // Send email notification
                try
                {
                    // Ensure email service is available
                    if (_emailService == null)
                    {
                        throw new InvalidOperationException("Email service is not initialized.");
                    }

                    await _emailService.SendEmailAsync(
                        user.Email,
                        "Ticket Booking Confirmation",
                        $@"Dear {user.FullName},

Your ticket has been successfully booked. Below are your booking details:

- Schedule: {schedule.Bus.Route.RouteName}
- Departure: {schedule.Bus.Route.Departure} at {schedule.DepartureTime}
- Destination: {schedule.Bus.Route.Destination}
- Bus: {schedule.Bus.PlateNo} ({schedule.Bus.Model})
- Amount Paid: {input.Amount} (via {input.PaymentMethod})

Thank you for choosing our service.

Best regards,
The Bus Transportation Management Team"
                    );
                }
                catch (Exception emailEx)
                {
                    // Log email sending failure but don't block the user flow
                    _logger.LogError($"Failed to send email for ticket {ticket.TicketId}: {emailEx.Message}");
                    TempData["WarningMessage"] = "Ticket booked successfully, but email notification could not be sent.";
                }

                // Redirect to confirmation page
                TempData["SuccessMessage"] = "Ticket booked and payment recorded successfully.";
                return RedirectToPage("/Dashboards/Tickets/Confirmation", new { ticketId = ticket.TicketId });
            }
            catch (Exception ex)
            {
                // Global exception handler
                _logger.LogError($"Unexpected error in ticket booking: {ex.Message}\n{ex.StackTrace}");
                TempData["ErrorMessage"] = "An unexpected error occurred. Please try again.";
                return Page();
            }
        }




        public class ScheduleViewModel
        {
            public int ScheduleId { get; set; }
            public string RouteName { get; set; }
            public string Departure { get; set; }
            public string Destination { get; set; }
            public string BusPlateNo { get; set; }
            public string BusModel { get; set; }
            public DateTime DepartureTime { get; set; }
            public DateTime ArrivalTime { get; set; }
            public int Amount { get; set; }
            public int AvailableSeats { get; set; }
        }

        public class BookTicketInput
        {
            public int ScheduleId { get; set; }
            public int Amount { get; set; }
            public string PaymentMethod { get; set; }
            public string PhoneNumber { get; set; }
        }

    }
}
