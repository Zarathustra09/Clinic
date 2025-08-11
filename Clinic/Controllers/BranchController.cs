using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.Models;
using Clinic.DataConnection;

namespace Clinic.Controllers
{
    [Route("Branch")]
    public class BranchController : BaseController
    {
        public BranchController(SqlServerDbContext context) : base(context)
        {
        }

        // GET: Branch
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var branches = await _context.Branches
                .Include(b => b.Appointments)
                .Select(b => new BranchDto
                {
                    Id = b.Id,
                    Name = b.Name,
                    Address = b.Address,
                    AppointmentCount = b.Appointments.Count
                })
                .ToListAsync();

            return View(branches);
        }

        // GET: Branch/Show/5
        [HttpGet("Show/{id:int}")]
        public async Task<IActionResult> Show(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Appointments)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return NotFound();

            var branchDto = new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                AppointmentCount = branch.Appointments?.Count ?? 0
            };

            // Pass the full branch entity for appointment details in the view
            ViewBag.BranchEntity = branch;

            return View(branchDto);
        }

        // GET: Branch/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new BranchDto());
        }

        // POST: Branch/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BranchDto branchDto)
        {
            if (ModelState.IsValid)
            {
                // Check if branch name already exists
                var existingBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Name == branchDto.Name);
                if (existingBranch != null)
                {
                    ModelState.AddModelError("Name", "Branch name already exists.");
                    return View(branchDto);
                }

                // Map DTO to entity
                var branch = new Branches
                {
                    Name = branchDto.Name,
                    Address = branchDto.Address
                };

                _context.Branches.Add(branch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(branchDto);
        }

        // GET: Branch/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches.FindAsync(id);
            if (branch == null) return NotFound();

            var branchDto = new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address
            };

            return View(branchDto);
        }

        // POST: Branch/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BranchDto branchDto)
        {
            if (id != branchDto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var branch = await _context.Branches.FindAsync(id);
                    if (branch == null) return NotFound();

                    // Check if branch name already exists (excluding current branch)
                    var existingBranch = await _context.Branches.FirstOrDefaultAsync(b => b.Name == branchDto.Name && b.Id != id);
                    if (existingBranch != null)
                    {
                        ModelState.AddModelError("Name", "Branch name already exists.");
                        return View(branchDto);
                    }

                    // Update entity properties
                    branch.Name = branchDto.Name;
                    branch.Address = branchDto.Address;

                    _context.Update(branch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Branches.Any(b => b.Id == id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(branchDto);
        }

        // GET: Branch/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var branch = await _context.Branches
                .Include(b => b.Appointments)
                .ThenInclude(a => a.User)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (branch == null) return NotFound();

            var branchDto = new BranchDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                AppointmentCount = branch.Appointments?.Count ?? 0
            };

            // Pass the full branch entity for appointment details in the view
            ViewBag.BranchEntity = branch;

            return View(branchDto);
        }

        // POST: Branch/Delete/5
        [HttpPost("Delete/{id:int}")]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var branch = await _context.Branches.Include(b => b.Appointments).FirstOrDefaultAsync(b => b.Id == id);
            if (branch != null)
            {
                // Check if branch has appointments
                if (branch.Appointments != null && branch.Appointments.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete branch with existing appointments.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Branches.Remove(branch);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}