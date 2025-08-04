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
    [Route("Student")]
    public class StudentController : Controller
    {
        private readonly SqlServerDbContext _context;

        public StudentController(SqlServerDbContext context)
        {
            _context = context;
        }

        // GET: Student
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var students = await _context.Users
                .Where(u => u.Role == 1)
                .Select(u => new StudentDto
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

            return View(students);
        }

        // GET: Student/Details/5
        [HttpGet("Show/{id:int}")]
        public async Task<IActionResult> Show(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user == null) return NotFound();

            var studentDto = new StudentDto
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

            return View(studentDto);
        }

        // GET: Student/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new StudentDto());
        }

        // POST: Student/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentDto studentDto)
        {
            if (ModelState.IsValid)
            {
                // Check if username already exists
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == studentDto.Username);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Username", "Username is already taken.");
                    return View(studentDto);
                }

                // Check if email already exists
                var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == studentDto.Email);
                if (existingEmail != null)
                {
                    ModelState.AddModelError("Email", "Email is already registered.");
                    return View(studentDto);
                }

                // Map DTO to User entity
                var user = new User
                {
                    SchoolID = studentDto.SchoolID,
                    FirstName = studentDto.FirstName,
                    MiddleName = studentDto.MiddleName,
                    LastName = studentDto.LastName,
                    Email = studentDto.Email,
                    Username = studentDto.Username,
                    PasswordHash = HashPassword(studentDto.Password),
                    Role = 1, // Student role
                    Program = studentDto.Program,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(studentDto);
        }

        // GET: Student/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user == null) return NotFound();

            var studentDto = new StudentDto
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

            return View(studentDto);
        }

        // POST: Student/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StudentDto studentDto)
        {
            if (id != studentDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(id);
                    if (user == null || user.Role != 1) return NotFound();

                    // Check if username already exists (excluding current user)
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == studentDto.Username && u.Id != id);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Username is already taken.");
                        return View(studentDto);
                    }

                    // Check if email already exists (excluding current user)
                    var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == studentDto.Email && u.Id != id);
                    if (existingEmail != null)
                    {
                        ModelState.AddModelError("Email", "Email is already registered.");
                        return View(studentDto);
                    }

                    // Update user properties
                    user.SchoolID = studentDto.SchoolID;
                    user.FirstName = studentDto.FirstName;
                    user.MiddleName = studentDto.MiddleName;
                    user.LastName = studentDto.LastName;
                    user.Email = studentDto.Email;
                    user.Username = studentDto.Username;
                    user.Program = studentDto.Program;
                    user.UpdatedAt = DateTime.UtcNow;
                    // Don't update password in edit unless specifically provided

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
            return View(studentDto);
        }

        // GET: Student/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 1);
            if (user == null) return NotFound();

            var studentDto = new StudentDto
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

            return View(studentDto);
        }

        // POST: Student/Delete/5
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