using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.Models;
using Clinic.DataConnection;

namespace Clinic.Controllers
{
    public class DoctorController : Controller
    {
        private readonly SqlServerDbContext _context;

        public DoctorController(SqlServerDbContext context)
        {
            _context = context;
        }

        // GET: Doctor
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Users.Where(u => u.Role == 2).ToListAsync();
            return View(doctors);
        }

        // GET: Doctor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 2);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // GET: Doctor/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User doctor)
        {
            if (ModelState.IsValid)
            {
                doctor.Role = 2;
                doctor.CreatedAt = DateTime.UtcNow;
                doctor.UpdatedAt = DateTime.UtcNow;
                _context.Users.Add(doctor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: Doctor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 2);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // POST: Doctor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User doctor)
        {
            if (id != doctor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    doctor.Role = 2;
                    doctor.UpdatedAt = DateTime.UtcNow;
                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Users.Any(u => u.Id == id && u.Role == 2))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(doctor);
        }

        // GET: Doctor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 2);
            if (doctor == null) return NotFound();

            return View(doctor);
        }

        // POST: Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == 2);
            if (doctor != null)
            {
                _context.Users.Remove(doctor);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}