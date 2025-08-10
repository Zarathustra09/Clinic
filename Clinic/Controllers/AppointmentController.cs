using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Clinic.Models;
using Clinic.DataConnection;
using System.Security.Claims;

namespace Clinic.Controllers
{
    [Route("Appointment")]
    public class AppointmentController : Controller
    {
        private readonly SqlServerDbContext _context;

        public AppointmentController(SqlServerDbContext context)
        {
            _context = context;
        }

    // GET: Appointment
    [HttpGet("")]
    [HttpGet("Index")]
    public async Task<IActionResult> Index()
    {
        await PopulateViewBags();
        
        // Pass current user info to view
        if (User.Identity.IsAuthenticated)
        {
            var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var currentUserRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (int.TryParse(currentUserIdClaim, out int currentUserId))
            {
                ViewBag.CurrentUserId = currentUserId;
                ViewBag.CurrentUserRole = currentUserRoleClaim;
                
                // Get current user details for display
                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId);
                if (currentUser != null)
                {
                    ViewBag.CurrentUserName = $"{currentUser.FirstName} {currentUser.LastName}";
                }
            }
        }
        
        return View();
    }

      [HttpGet("GetAppointments")]
      public async Task<JsonResult> GetAppointments(DateTime? start = null, DateTime? end = null)
      {
          try
          {
              var currentUserId = 0;
              var currentUserRole = "";
              
              if (User.Identity.IsAuthenticated)
              {
                  var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                  var currentUserRoleClaim = User.FindFirst(ClaimTypes.Role)?.Value;
      
                  int.TryParse(currentUserIdClaim, out currentUserId);
                  currentUserRole = currentUserRoleClaim ?? "";
              }
      
              if (string.IsNullOrEmpty(currentUserRole) || currentUserRole == "Campus Clinic" || currentUserRole == "2")
              {
                  var allQuery = from a in _context.Appointments
                                join u in _context.Users on a.UserId equals u.Id
                                join d in _context.Users on a.DoctorId equals d.Id
                                join b in _context.Branches on a.BranchId equals b.Id
                                join ts in _context.TimeSlots on a.TimeSlotId equals ts.Id
                                where (!start.HasValue || !end.HasValue || (ts.StartTime >= start.Value && ts.EndTime <= end.Value))
                                select new
                                {
                                    id = a.Id,
                                    title = (u.FirstName ?? "") + " " + (u.LastName ?? "") + 
                                           (string.IsNullOrEmpty(a.Reason) ? "" : " - " + a.Reason),
                                    start = ts.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                    end = ts.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                    backgroundColor = a.IsApproved ? GetAppointmentColor(a.BranchId) : "#6c757d",
                                    borderColor = a.IsApproved ? GetAppointmentColor(a.BranchId) : "#6c757d",
                                    className = a.IsApproved ? "" : "pending-appointment",
                                    extendedProps = new
                                    {
                                        userId = a.UserId,
                                        doctorId = a.DoctorId,
                                        branchId = a.BranchId,
                                        reason = a.Reason ?? "",
                                        userFullName = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                                        doctorName = (d.FirstName ?? "") + " " + (d.LastName ?? ""),
                                        branchName = b.Name ?? "",
                                        timeSlotId = a.TimeSlotId,
                                        isApproved = a.IsApproved
                                    }
                                };
      
                  var appointments = await allQuery.ToListAsync();
                  return Json(appointments);
              }
              else
              {
                  // Similar update for filtered query...
                  var filteredQuery = from a in _context.Appointments
                                     join u in _context.Users on a.UserId equals u.Id
                                     join d in _context.Users on a.DoctorId equals d.Id
                                     join b in _context.Branches on a.BranchId equals b.Id
                                     join ts in _context.TimeSlots on a.TimeSlotId equals ts.Id
                                     where (currentUserRole == "Student" || currentUserRole == "0") ? a.UserId == currentUserId :
                                           (currentUserRole == "Doctor" || currentUserRole == "1") ? a.DoctorId == currentUserId : true
                                     where (!start.HasValue || !end.HasValue || (ts.StartTime >= start.Value && ts.EndTime <= end.Value))
                                     select new
                                     {
                                         id = a.Id,
                                         title = (u.FirstName ?? "") + " " + (u.LastName ?? "") + 
                                                (string.IsNullOrEmpty(a.Reason) ? "" : " - " + a.Reason),
                                         start = ts.StartTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                         end = ts.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                                         backgroundColor = a.IsApproved ? GetAppointmentColor(a.BranchId) : "#6c757d",
                                         borderColor = a.IsApproved ? GetAppointmentColor(a.BranchId) : "#6c757d",
                                         className = a.IsApproved ? "" : "pending-appointment",
                                         extendedProps = new
                                         {
                                             userId = a.UserId,
                                             doctorId = a.DoctorId,
                                             branchId = a.BranchId,
                                             reason = a.Reason ?? "",
                                             userFullName = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                                             doctorName = (d.FirstName ?? "") + " " + (d.LastName ?? ""),
                                             branchName = b.Name ?? "",
                                             timeSlotId = a.TimeSlotId,
                                             isApproved = a.IsApproved
                                         }
                                     };
      
                  var appointments = await filteredQuery.ToListAsync();
                  return Json(appointments);
              }
          }
          catch (Exception ex)
          {
              Console.WriteLine($"Error loading appointments: {ex.Message}");
              Console.WriteLine($"Stack trace: {ex.StackTrace}");
              return Json(new List<object>());
          }
      }
      
      [HttpPost("Approve/{id}")]
      public async Task<JsonResult> ApproveAppointment(int id)
      {
          try
          {
              var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id);
              
              if (appointment == null)
              {
                  return Json(new { success = false, message = "Appointment not found." });
              }
      
              appointment.IsApproved = true;
              await _context.SaveChangesAsync();
      
              return Json(new { success = true, message = "Appointment approved successfully!" });
          }
          catch (Exception ex)
          {
              Console.WriteLine($"Error approving appointment: {ex.Message}");
              return Json(new { success = false, message = "An error occurred while approving the appointment." });
          }
      }
       
       [HttpGet("GetAppointment/{id}")]
       public async Task<JsonResult> GetAppointment(int id)
       {
           try
           {
               var appointmentQuery = from a in _context.Appointments
                                     join u in _context.Users on a.UserId equals u.Id
                                     join d in _context.Users on a.DoctorId equals d.Id
                                     join b in _context.Branches on a.BranchId equals b.Id
                                     join ts in _context.TimeSlots on a.TimeSlotId equals ts.Id
                                     where a.Id == id
                                     select new
                                     {
                                         a.Id,
                                         a.UserId,
                                         a.DoctorId,
                                         a.BranchId,
                                         a.Reason,
                                         a.TimeSlotId,
                                         UserFullName = (u.FirstName ?? "") + " " + (u.LastName ?? ""),
                                         DoctorName = (d.FirstName ?? "") + " " + (d.LastName ?? ""),
                                         BranchName = b.Name ?? "",
                                         StartTime = ts.StartTime,
                                         EndTime = ts.EndTime,
                                         FormattedTimeRange = ts.StartTime.ToString("MMM d, yyyy h:mm tt") + " - " + ts.EndTime.ToString("h:mm tt")
                                     };
       
               var appointment = await appointmentQuery.FirstOrDefaultAsync();
       
               if (appointment == null)
               {
                   return Json(new { success = false, message = "Appointment not found" });
               }
       
               var result = new AppointmentDto
               {
                   Id = appointment.Id,
                   UserId = appointment.UserId,
                   DoctorId = appointment.DoctorId,
                   BranchId = appointment.BranchId,
                   Reason = appointment.Reason,
                   TimeSlotId = appointment.TimeSlotId,
                   UserFullName = appointment.UserFullName,
                   DoctorName = appointment.DoctorName,
                   BranchName = appointment.BranchName,
                   FormattedTimeRange = appointment.FormattedTimeRange,
                   StartTime = appointment.StartTime,
                   EndTime = appointment.EndTime
               };
       
               return Json(new { success = true, data = result });
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Error getting appointment: {ex.Message}");
               return Json(new { success = false, message = "Error retrieving appointment" });
           }
       }

        // API: Create appointment
        // API: Create appointment
        [HttpPost("Create")]
        public async Task<JsonResult> Create([FromBody] AppointmentDto appointmentDto)
        {
            try
            {
                // Debug log the received data
                Console.WriteLine($"=== CREATE APPOINTMENT DEBUG ===");
                Console.WriteLine($"Received appointmentDto:");
                Console.WriteLine($"  UserId: {appointmentDto.UserId}");
                Console.WriteLine($"  DoctorId: {appointmentDto.DoctorId}");
                Console.WriteLine($"  BranchId: {appointmentDto.BranchId}");
                Console.WriteLine($"  TimeSlotId: {appointmentDto.TimeSlotId}");
                Console.WriteLine($"  Reason: '{appointmentDto.Reason}'");

                // Check if appointmentDto is null
                if (appointmentDto == null)
                {
                    Console.WriteLine("ERROR: appointmentDto is null");
                    return Json(new { success = false, errors = new[] { "Invalid appointment data received." } });
                }

                // Remove validation errors for display properties that are populated server-side
                var displayProperties = new[] { "BranchName", "DoctorName", "UserFullName", "FormattedTimeRange", "StartTime", "EndTime", "Title", "Start", "End" };
                foreach (var prop in displayProperties)
                {
                    if (ModelState.ContainsKey(prop))
                    {
                        ModelState.Remove(prop);
                    }
                }

                // Validate ModelState and provide detailed error information
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState is INVALID:");
                    var allErrors = new List<string>();

                    foreach (var key in ModelState.Keys)
                    {
                        var state = ModelState[key];
                        if (state.Errors.Count > 0)
                        {
                            Console.WriteLine($"  Field '{key}':");
                            foreach (var error in state.Errors)
                            {
                                Console.WriteLine($"    - {error.ErrorMessage}");
                                allErrors.Add($"{key}: {error.ErrorMessage}");
                            }
                        }
                    }

                    return Json(new { success = false, errors = allErrors });
                }

                Console.WriteLine("ModelState is VALID - proceeding with validations");

                // Validate required fields manually (as backup)
                var validationErrors = new List<string>();

                if (appointmentDto.UserId <= 0)
                {
                    validationErrors.Add("Invalid patient selection.");
                }

                if (appointmentDto.DoctorId <= 0)
                {
                    validationErrors.Add("Invalid doctor selection.");
                }

                if (appointmentDto.BranchId <= 0)
                {
                    validationErrors.Add("Invalid branch selection.");
                }

                if (appointmentDto.TimeSlotId <= 0)
                {
                    validationErrors.Add("Invalid time slot selection.");
                }

                if (validationErrors.Any())
                {
                    Console.WriteLine($"Manual validation failed: {string.Join(", ", validationErrors)}");
                    return Json(new { success = false, errors = validationErrors });
                }

                // Validate that the time slot exists and is available
                Console.WriteLine($"Looking up TimeSlot with ID: {appointmentDto.TimeSlotId}");

                var timeSlot = await _context.TimeSlots
                    .Include(ts => ts.Doctor)
                    .FirstOrDefaultAsync(ts => ts.Id == appointmentDto.TimeSlotId);

                if (timeSlot == null)
                {
                    Console.WriteLine($"TimeSlot with ID {appointmentDto.TimeSlotId} NOT FOUND");

                    // Debug: Show what time slots exist
                    var allTimeSlots = await _context.TimeSlots
                        .Select(ts => new { ts.Id, ts.DoctorId, ts.StartTime, ts.EndTime, ts.AppointmentId })
                        .ToListAsync();

                    Console.WriteLine($"Available time slots in database ({allTimeSlots.Count}):");
                    foreach (var ts in allTimeSlots)
                    {
                        Console.WriteLine($"  ID: {ts.Id}, DoctorId: {ts.DoctorId}, Start: {ts.StartTime}, Available: {ts.AppointmentId == null}");
                    }

                    return Json(new { success = false, errors = new[] { "Selected time slot does not exist." } });
                }

                Console.WriteLine($"TimeSlot found: ID={timeSlot.Id}, DoctorId={timeSlot.DoctorId}, Available={timeSlot.IsAvailable}");

                if (!timeSlot.IsAvailable)
                {
                    Console.WriteLine($"TimeSlot {timeSlot.Id} is NOT AVAILABLE (AppointmentId: {timeSlot.AppointmentId})");
                    return Json(new { success = false, errors = new[] { "Selected time slot is no longer available." } });
                }

                // Validate that the appointment is not in the past
                if (timeSlot.StartTime < DateTime.Now)
                {
                    Console.WriteLine($"TimeSlot {timeSlot.Id} is in the PAST (StartTime: {timeSlot.StartTime}, Now: {DateTime.Now})");
                    return Json(new { success = false, errors = new[] { "Cannot book appointments in the past." } });
                }

                // Validate that the doctor matches the time slot's doctor
                if (appointmentDto.DoctorId != timeSlot.DoctorId)
                {
                    Console.WriteLine($"Doctor mismatch: Selected={appointmentDto.DoctorId}, TimeSlot={timeSlot.DoctorId}");
                    return Json(new { success = false, errors = new[] { "Selected doctor does not match the time slot's doctor." } });
                }

                // Validate that the user exists and is a valid patient
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == appointmentDto.UserId);
                if (user == null)
                {
                    Console.WriteLine($"User {appointmentDto.UserId} NOT FOUND");
                    return Json(new { success = false, errors = new[] { "Selected patient does not exist." } });
                }

                if (user.Role != 0 && user.Role != 2) // Only students and campus clinic staff can be patients
                {
                    Console.WriteLine($"User {appointmentDto.UserId} has invalid role for patient: {user.Role}");
                    return Json(new { success = false, errors = new[] { "Selected user cannot be a patient." } });
                }

                // Validate that the doctor exists and is a valid doctor
                var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == appointmentDto.DoctorId);
                if (doctor == null)
                {
                    Console.WriteLine($"Doctor {appointmentDto.DoctorId} NOT FOUND");
                    return Json(new { success = false, errors = new[] { "Selected doctor does not exist." } });
                }

                if (doctor.Role != 1) // Only users with role 1 are doctors
                {
                    Console.WriteLine($"User {appointmentDto.DoctorId} is not a doctor (Role: {doctor.Role})");
                    return Json(new { success = false, errors = new[] { "Selected user is not a doctor." } });
                }

                // Validate that the branch exists
                var branch = await _context.Branches.FirstOrDefaultAsync(b => b.Id == appointmentDto.BranchId);
                if (branch == null)
                {
                    Console.WriteLine($"Branch {appointmentDto.BranchId} NOT FOUND");
                    return Json(new { success = false, errors = new[] { "Selected branch does not exist." } });
                }

                Console.WriteLine("All validations PASSED - checking for conflicts");

                // Check if patient already has a conflicting appointment
                var hasConflict = await _context.Appointments
                    .Include(a => a.TimeSlot)
                    .AnyAsync(a => a.UserId == appointmentDto.UserId &&
                                 a.TimeSlot.StartTime < timeSlot.EndTime &&
                                 a.TimeSlot.EndTime > timeSlot.StartTime);

                if (hasConflict)
                {
                    Console.WriteLine($"Patient {appointmentDto.UserId} has CONFLICTING appointment");
                    return Json(new { success = false, errors = new[] { "This patient already has a conflicting appointment." } });
                }

                Console.WriteLine("No conflicts found - creating appointment");

                // Create appointment
                var appointment = new Appointment
                {
                    UserId = appointmentDto.UserId,
                    DoctorId = appointmentDto.DoctorId,
                    BranchId = appointmentDto.BranchId,
                    Reason = appointmentDto.Reason ?? string.Empty,
                    TimeSlotId = appointmentDto.TimeSlotId
                };

                Console.WriteLine("Adding appointment to context");
                _context.Appointments.Add(appointment);

                Console.WriteLine("Saving appointment to database");
                await _context.SaveChangesAsync();

                Console.WriteLine($"Appointment created with ID: {appointment.Id}");

                // Update the time slot to mark it as booked
                Console.WriteLine($"Updating TimeSlot {timeSlot.Id} to mark as booked");
                timeSlot.AppointmentId = appointment.Id;

                Console.WriteLine("Saving TimeSlot update");
                await _context.SaveChangesAsync();

                Console.WriteLine("=== APPOINTMENT CREATED SUCCESSFULLY ===");
                return Json(new { success = true, message = "Appointment created successfully!", appointmentId = appointment.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"=== EXCEPTION IN CREATE APPOINTMENT ===");
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return Json(new { success = false, errors = new[] { $"An error occurred while creating the appointment: {ex.Message}" } });
            }
        }

       [HttpPost("Update")]
       public async Task<JsonResult> Update([FromBody] AppointmentDto appointmentDto)
       {
           try
           {
               // Remove validation errors for display properties that are populated server-side
               var displayProperties = new[] { "BranchName", "DoctorName", "UserFullName", "FormattedTimeRange", "StartTime", "EndTime", "Title", "Start", "End" };
               foreach (var prop in displayProperties)
               {
                   if (ModelState.ContainsKey(prop))
                   {
                       ModelState.Remove(prop);
                   }
               }
       
               if (!ModelState.IsValid)
               {
                   var errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage));
                   return Json(new { success = false, errors = errors });
               }
       
               // Use explicit query instead of navigation properties
               var appointmentQuery = from a in _context.Appointments
                                     join ts in _context.TimeSlots on a.TimeSlotId equals ts.Id
                                     where a.Id == appointmentDto.Id
                                     select new { a, ts };
       
               var appointmentData = await appointmentQuery.FirstOrDefaultAsync();
       
               if (appointmentData == null)
               {
                   return Json(new { success = false, errors = new[] { "Appointment not found." } });
               }
       
               var appointment = appointmentData.a;
               var currentTimeSlot = appointmentData.ts;
       
               // If changing time slot, validate the new one
               if (appointment.TimeSlotId != appointmentDto.TimeSlotId)
               {
                   var newTimeSlot = await _context.TimeSlots.FirstOrDefaultAsync(ts => ts.Id == appointmentDto.TimeSlotId);
       
                   if (newTimeSlot == null)
                   {
                       return Json(new { success = false, errors = new[] { "Selected time slot does not exist." } });
                   }
       
                   if (!newTimeSlot.IsAvailable)
                   {
                       return Json(new { success = false, errors = new[] { "Selected time slot is no longer available." } });
                   }
       
                   if (newTimeSlot.StartTime < DateTime.Now)
                   {
                       return Json(new { success = false, errors = new[] { "Cannot book appointments in the past." } });
                   }
       
                   // Validate that the doctor matches the time slot's doctor
                   if (appointmentDto.DoctorId != newTimeSlot.DoctorId)
                   {
                       return Json(new { success = false, errors = new[] { "Selected doctor does not match the time slot's doctor." } });
                   }
       
                   // Check for conflicts with the new time slot (excluding current appointment)
                   var hasConflict = await (from a in _context.Appointments
                                          join ts in _context.TimeSlots on a.TimeSlotId equals ts.Id
                                          where a.UserId == appointmentDto.UserId &&
                                                a.Id != appointmentDto.Id &&
                                                ((ts.StartTime >= newTimeSlot.StartTime && ts.StartTime < newTimeSlot.EndTime) ||
                                                 (ts.EndTime > newTimeSlot.StartTime && ts.EndTime <= newTimeSlot.EndTime) ||
                                                 (ts.StartTime <= newTimeSlot.StartTime && ts.EndTime >= newTimeSlot.EndTime))
                                          select a).AnyAsync();
       
                   if (hasConflict)
                   {
                       return Json(new { success = false, errors = new[] { "This patient already has a conflicting appointment." } });
                   }
       
                   // Free up the old time slot
                   currentTimeSlot.AppointmentId = null;
       
                   // Book the new time slot
                   newTimeSlot.AppointmentId = appointment.Id;
                   appointment.TimeSlotId = appointmentDto.TimeSlotId;
               }
       
               // Update appointment details
               appointment.UserId = appointmentDto.UserId;
               appointment.DoctorId = appointmentDto.DoctorId;
               appointment.BranchId = appointmentDto.BranchId;
               appointment.Reason = appointmentDto.Reason;
       
               await _context.SaveChangesAsync();
       
               return Json(new { success = true, message = "Appointment updated successfully!" });
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Error updating appointment: {ex.Message}");
               Console.WriteLine($"Stack trace: {ex.StackTrace}");
               return Json(new { success = false, errors = new[] { "An error occurred while updating the appointment." } });
           }
       }

        [HttpPost("Delete/{id}")]
        public async Task<JsonResult> Delete(int id)
        {
            try
            {
                // Use explicit query to avoid navigation property issues
                var appointmentQuery = from a in _context.Appointments
                    join ts in _context.TimeSlots on a.TimeSlotId equals ts.Id
                    where a.Id == id
                    select new { a, ts };

                var appointmentData = await appointmentQuery.FirstOrDefaultAsync();

                if (appointmentData == null)
                {
                    return Json(new { success = false, errors = new[] { "Appointment not found." } });
                }

                var appointment = appointmentData.a;
                var timeSlot = appointmentData.ts;

                // CRITICAL: Free up the time slot FIRST to break the circular reference
                timeSlot.AppointmentId = null;

                // Save the time slot change first
                await _context.SaveChangesAsync();

                // Now safely remove the appointment
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Appointment deleted successfully!" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting appointment: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return Json(new { success = false, errors = new[] { "An error occurred while deleting the appointment." } });
            }
        }

        // API: Get available time slots for a specific doctor
        [HttpGet("GetAvailableTimeSlots")]
        public async Task<JsonResult> GetAvailableTimeSlots(int? doctorId = null, DateTime? date = null)
        {
            try
            {
                var query = _context.TimeSlots
                    .Include(ts => ts.Doctor)
                    .Where(ts => ts.AppointmentId == null); // Only available slots

                // Filter by doctor if specified
                if (doctorId.HasValue)
                {
                    query = query.Where(ts => ts.DoctorId == doctorId.Value);
                }

                // Filter by date if specified
                if (date.HasValue)
                {
                    var startOfDay = date.Value.Date;
                    var endOfDay = startOfDay.AddDays(1);
                    query = query.Where(ts => ts.StartTime >= startOfDay && ts.StartTime < endOfDay);
                }

                // Only show future time slots
                query = query.Where(ts => ts.StartTime > DateTime.Now);

                var timeSlots = await query
                    .OrderBy(ts => ts.StartTime)
                    .Select(ts => new
                    {
                        id = ts.Id,
                        doctorId = ts.DoctorId,
                        doctorName = $"{ts.Doctor.FirstName} {ts.Doctor.LastName}",
                        startTime = ts.StartTime,
                        endTime = ts.EndTime,
                        formattedTime = $"{ts.StartTime:MMM dd, yyyy h:mm tt} - {ts.EndTime:h:mm tt}",
                        date = ts.StartTime.Date,
                        isAvailable = ts.IsAvailable
                    })
                    .ToListAsync();

                return Json(timeSlots);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading available time slots: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        // Helper methods
        private async Task PopulateViewBags()
        {
            ViewBag.Users = await _context.Users
                .Where(u => u.Role == 0 || u.Role == 2) // Students and Campus Clinic staff
                .Select(u => new { u.Id, FullName = $"{u.FirstName} {u.LastName}" })
                .ToListAsync();

            ViewBag.Branches = await _context.Branches
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            ViewBag.Doctors = await _context.Users
                .Where(u => u.Role == 1) // Doctors
                .Select(u => new { u.Id, FullName = $"{u.FirstName} {u.LastName}" })
                .ToListAsync();
        }

        private static string GetAppointmentColor(int branchId)
        {
            var colors = new[] { "#007bff", "#28a745", "#dc3545", "#ffc107", "#17a2b8", "#6f42c1" };
            return colors[branchId % colors.Length];
        }
    }
}