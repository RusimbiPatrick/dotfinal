using bus_transport_mgt_sys.Data;
using bus_transport_mgt_sys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace bus_transport_mgt_sys.Pages.Authentication
{
    public class RegisterModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(AppDbContext context, ILogger<RegisterModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public User User { get; set; } = new User(); // Initialize to prevent null reference

        [BindProperty]
        public string ConfirmPassword { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // Validate input
            if (User == null)
            {
                ModelState.AddModelError(string.Empty, "User data is required.");
                return Page();
            }

            // Validate email uniqueness
            if (_context.Users.Any(u => u.Email == User.Email))
            {
                ModelState.AddModelError("User.Email", "Email is already registered.");
            }

            // Validate Full Name
            if (string.IsNullOrWhiteSpace(User.FullName))
            {
                ModelState.AddModelError("User.FullName", "Full Name is required.");
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(User.PasswordHash) || User.PasswordHash.Length < 8)
            {
                ModelState.AddModelError("User.PasswordHash", "Password must be at least 8 characters long.");
            }

            // Validate Confirm Password
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "Confirm Password is required.");
            }
            else if (User.PasswordHash != ConfirmPassword)
            {
                ModelState.AddModelError(nameof(ConfirmPassword), "Passwords do not match.");
            }

            // Check overall model validity
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Hash the password
                User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(User.PasswordHash);

                try
                {
                    _context.Users.Add(User);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database update failed.");
                    ModelState.AddModelError(string.Empty, "An error occurred while saving to the database.");
                    return Page();
                }

                // Log successful registration
                _logger.LogInformation($"New user registered: {User.Email}");

                // Set success message
                TempData["SuccessMessage"] = "Registration successful! Please log in.";

                // Redirect to login page
                return RedirectToPage("/Authentication/Login");
            }
            catch (Exception ex)
            {
                // Log the full exception for debugging
                _logger.LogError(ex, "Error during user registration");

                // Set a user-friendly error message
                ErrorMessage = "An unexpected error occurred. Please try again or contact support.";
                return Page();
            }
        }
    }
}