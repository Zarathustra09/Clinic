using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.Models;
using Clinic.DataConnection;

namespace Clinic.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, SqlServerDbContext context) : base(context)
    {
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // Get peak hours analytics data
        var peakHoursData = await GetPeakHoursAnalytics();
        ViewBag.PeakHoursData = peakHoursData;

        return View();
    }

    private async Task<object> GetPeakHoursAnalytics()
    {
        var appointments = await _context.Appointments
            .Include(a => a.TimeSlot)
            .Include(a => a.Branch)
            .Where(a => (a.Status == AppointmentStatus.Approved || a.Status == AppointmentStatus.Finished) && a.TimeSlot != null)
            .ToListAsync();

        // Group by branch and hour
        var peakHours = appointments
            .GroupBy(a => new {
                BranchId = a.BranchId,
                BranchName = a.Branch.Name,
                Hour = a.TimeSlot.StartTime.Hour
            })
            .Select(g => new {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.BranchName,
                Hour = g.Key.Hour,
                AppointmentCount = g.Count()
            })
            .OrderBy(x => x.BranchName)
            .ThenBy(x => x.Hour)
            .ToList();

        // Get branch totals
        var branchTotals = appointments
            .GroupBy(a => new { a.BranchId, a.Branch.Name })
            .Select(g => new {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.Name,
                TotalAppointments = g.Count(),
                PeakHour = g.GroupBy(a => a.TimeSlot.StartTime.Hour)
                           .OrderByDescending(h => h.Count())
                           .FirstOrDefault()?.Key ?? 0
            })
            .ToList();

        return new {
            PeakHours = peakHours,
            BranchTotals = branchTotals,
            TotalAppointments = appointments.Count
        };
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}