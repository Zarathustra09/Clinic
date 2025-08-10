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

                // Auto-generate School ID
                string schoolId = await GenerateUniqueSchoolId();

                // Create new user
                var user = new User
                {
                    SchoolID = schoolId, // Auto-generated
                    FirstName = model.FirstName,
                    MiddleName = model.MiddleName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = HashPassword(model.Password),
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

        private async Task<string> GenerateUniqueSchoolId()
        {
            string schoolId;
            bool isUnique = false;
            int maxAttempts = 10; // Prevent infinite loop
            int attempts = 0;

            do
            {
                // Generate School ID: CurrentYear + 8 random digits
                string currentYear = DateTime.Now.Year.ToString();
                string randomNumbers = GenerateRandomNumbers(8);
                schoolId = currentYear + randomNumbers;

                // Check if this School ID already exists
                var existingSchoolId = await _context.Users.FirstOrDefaultAsync(u => u.SchoolID == schoolId);
                isUnique = existingSchoolId == null;
                attempts++;

            } while (!isUnique && attempts < maxAttempts);

            if (!isUnique)
            {
                // Fallback: use timestamp to ensure uniqueness
                string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                schoolId = DateTime.Now.Year.ToString() + timestamp.Substring(Math.Max(0, timestamp.Length - 8));
            }

            return schoolId;
        }

        private string GenerateRandomNumbers(int length)
        {
            var random = new Random();
            var numbers = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                numbers[i] = (char)('0' + random.Next(0, 10));
            }
            
            return new string(numbers);
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