using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.Models;
using Clinic.DataConnection;
using System.Security.Cryptography;
using System.Text;

namespace Clinic.Controllers
{
    public class RegistrationController : Controller
    {
        private readonly SqlServerDbContext _context;

        public RegistrationController(SqlServerDbContext context)
        {
            _context = context;
        }

        // GET: Registration
        public IActionResult Index()
        {
            return View();
        }

        // POST: Registration
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(RegisterDto model)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(model);
                }

                // Check if email already exists
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(model);
                }

                // Create new user
                var user = new User
                {
                    SchoolID = model.SchoolID,
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password), // Hash the password
                    Role = model.Role,
                    Program = model.Program,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Registration successful, redirect to login
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Index", "Login");
            }

            // If registration fails, return the registration view with validation errors
            return View(model);
        }

        private string HashPassword(string password)
        {
            // Simple password hashing - in production, use more secure methods like BCrypt
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}