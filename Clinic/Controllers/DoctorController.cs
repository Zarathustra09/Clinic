using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.Models;
using Clinic.DataConnection;
using System.Security.Cryptography;
using System.Text;

namespace Clinic.Controllers
{
    [Route("Doctor")]
    public class DoctorController : Controller
    {
        private readonly SqlServerDbContext _context;

        public DoctorController(SqlServerDbContext context)
        {
            _context = context;
        }

        // GET: Doctor
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Users
                .Where(u => u.Role == 1)
                .Select(u => new DoctorDto
                {
                    Id = u.Id,
                    SchoolID = u.SchoolID,
                    FirstName = u.FirstName,
                    MiddleName = u.MiddleName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Username = u.Username,
                    Program = u.Program,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .ToListAsync();

            return View(doctors);
        }

        // GET: Doctor/Details/5
        [HttpGet("Show/{id:int}")]
        public async Task<IActionResult> Show(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user == null) return NotFound();

            var doctorDto = new DoctorDto
            {
                Id = user.Id,
                SchoolID = user.SchoolID,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Program = user.Program,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View(doctorDto);
        }

        // GET: Doctor/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new DoctorDto());
        }

        // POST: Doctor/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorDto doctorDto)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == doctorDto.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(doctorDto);
                }

                // Check if email already exists
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == doctorDto.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(doctorDto);
                }

                // Map DTO to User entity
                var user = new User
                {
                    SchoolID = doctorDto.SchoolID,
                    FirstName = doctorDto.FirstName,
                    MiddleName = doctorDto.MiddleName,
                    LastName = doctorDto.LastName,
                    Email = doctorDto.Email,
                    Username = doctorDto.Username,
                    PasswordHash = HashPassword(doctorDto.Password),
                    Role = 1, // Doctor role
                    Program = doctorDto.Program,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctorDto);
        }

        // GET: Doctor/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user == null) return NotFound();

            var doctorDto = new DoctorDto
            {
                Id = user.Id,
                SchoolID = user.SchoolID,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Program = user.Program,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View(doctorDto);
        }

        // POST: Doctor/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorDto doctorDto)
        {
            if (id != doctorDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null || user.Role != 1) return NotFound();

                    // Check if username already exists (excluding current user)
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == doctorDto.Username && u.Id != id);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Username is already taken.");
                        return View(doctorDto);
                    }

                    // Check if email already exists (excluding current user)
                    var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == doctorDto.Email && u.Id != id);
                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email is already registered.");
                        return View(doctorDto);
                    }

                    // Update user properties
                    user.SchoolID = doctorDto.SchoolID;
                    user.FirstName = doctorDto.FirstName;
                    user.MiddleName = doctorDto.MiddleName;
                    user.LastName = doctorDto.LastName;
                    user.Email = doctorDto.Email;
                    user.Username = doctorDto.Username;
                    user.Program = doctorDto.Program;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(u => u.Id == id && u.Role == 1))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(doctorDto);
        }

        // GET: Doctor/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user == null) return NotFound();

            var doctorDto = new DoctorDto
            {
                Id = user.Id,
                SchoolID = user.SchoolID,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Program = user.Program,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View(doctorDto);
        }

        // POST: Doctor/Delete/5
        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}