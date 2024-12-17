using bus_transport_mgt_sys.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace bus_transport_mgt_sys.Pages.Authentication
{
    public class LoginModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(AppDbContext context,
                          IHttpContextAccessor httpContextAccessor,
                          ILogger<LoginModel> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string ErrorMessage { get; private set; }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Find user by email
                var user = _context.Users.FirstOrDefault(u => u.Email == Email);

                if (user == null)
                {
                    // Log failed login attempt
                    _logger.LogWarning($"Login attempt failed for email: {Email} - User not found");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return Page();
                }

                // Verify password
                if (!BCrypt.Net.BCrypt.Verify(Password, user.PasswordHash))
                {
                    // Log failed password verification
                    _logger.LogWarning($"Failed password attempt for user: {Email}");
                    ModelState.AddModelError(string.Empty, "Invalid login attempt");
                    return Page();
                }

                // Create authentication claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.FullName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("UserId", user.UserId.ToString())
                };

                // Create claims identity
                var claimsIdentity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                // Authentication properties
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // Non-persistent cookie
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2), // Match Program.cs cookie expiration
                    RedirectUri = "/Index" // Fallback redirect
                };

                // Sign in the user
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                // Comprehensive session management
                HttpContext.Session.SetString("UserEmail", Email);
                //HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserRole", user.Role);
                HttpContext.Session.SetString("UserFullName", user.FullName);

                // Log successful login
                _logger.LogInformation($"User {Email} logged in successfully at {DateTime.UtcNow}");

                // Redirect to appropriate dashboard
                return RedirectToDashboard(user.Role);
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                _logger.LogError(ex, $"Unexpected error during login for email: {Email}");

                ModelState.AddModelError(string.Empty, "An unexpected system error occurred. Please try again later.");
                return Page();
            }
        }

        private IActionResult RedirectToDashboard(string role)
        {
            // Improved role-based redirection with logging
            _logger.LogInformation($"Redirecting user with role {role} to dashboard");

            return role.ToLower() switch
            {
                "admin" => RedirectToPage("/Dashboards/AdminDashboard"),
                "user" => RedirectToPage("/Dashboards/ClientDashboard"),
                "driver" => RedirectToPage("/Dashboards/DriverDasboard"),
                _ => RedirectToPage("/Index") // Default redirect with logging
            };
        }
    }
}