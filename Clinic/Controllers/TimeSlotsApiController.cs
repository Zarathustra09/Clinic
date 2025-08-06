using Clinic.DataConnection;
using Clinic.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Clinic.Controllers
{
    [Route("api/timeslots")]
    [ApiController]
    public class TimeSlotsApiController : ControllerBase
    {
        private readonly SqlServerDbContext _context;

        public TimeSlotsApiController(SqlServerDbContext context)
        {
            _context = context;
        }

        // TEST: Raw SQL query to see what's in the database
        [HttpGet("debug/{doctorId}")]
        public async Task<ActionResult> DebugTimeSlots(int doctorId)
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                var results = new List<object>();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        SELECT Id, DoctorId, StartTime, EndTime, AppointmentId 
                        FROM [Clinic].[TimeSlot] 
                        WHERE DoctorId = @DoctorId 
                        ORDER BY StartTime ASC";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                results.Add(new
                                {
                                    Id = reader.IsDBNull("Id") ? (int?)null : reader.GetInt32("Id"),
                                    DoctorId = reader.IsDBNull("DoctorId") ? (int?)null : reader.GetInt32("DoctorId"),
                                    StartTime = reader.IsDBNull("StartTime") ? (DateTime?)null : reader.GetDateTime("StartTime"),
                                    EndTime = reader.IsDBNull("EndTime") ? (DateTime?)null : reader.GetDateTime("EndTime"),
                                    AppointmentId = reader.IsDBNull("AppointmentId") ? (int?)null : reader.GetInt32("AppointmentId")
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    DoctorId = doctorId,
                    ResultCount = results.Count,
                    Results = results
                });
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        // GET: api/timeslots/doctor/{doctorId} - FIXED VERSION
        [HttpGet("doctor/{doctorId}")]
        public async Task<ActionResult<IEnumerable<TimeSlotDto>>> GetTimeSlotsByDoctor(int doctorId)
        {
            try
            {
                Console.WriteLine($"Getting time slots for doctor ID: {doctorId}");

                // First, let's check if the doctor exists
                var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == doctorId && u.Role == 1);
                if (doctor == null)
                {
                    Console.WriteLine($"Doctor with ID {doctorId} not found or not a doctor");
                    return BadRequest("Doctor not found");
                }

                Console.WriteLine($"Doctor found: {doctor.FirstName} {doctor.LastName}");

                // Use raw SQL to avoid Entity Framework NULL issues
                var connectionString = _context.Database.GetConnectionString();
                var timeSlotDtos = new List<TimeSlotDto>();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        SELECT ts.Id, ts.DoctorId, ts.StartTime, ts.EndTime, ts.AppointmentId,
                               u.FirstName, u.LastName, a.Reason
                        FROM [Clinic].[TimeSlot] ts
                        INNER JOIN [Clinic].[User] u ON ts.DoctorId = u.Id
                        LEFT JOIN [Clinic].[Appointment] a ON ts.AppointmentId = a.Id
                        WHERE ts.DoctorId = @DoctorId 
                        AND ts.Id IS NOT NULL 
                        AND ts.DoctorId IS NOT NULL 
                        AND ts.StartTime IS NOT NULL 
                        AND ts.EndTime IS NOT NULL
                        ORDER BY ts.StartTime ASC";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", doctorId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var dto = new TimeSlotDto
                                {
                                    Id = reader.GetInt32("Id"),
                                    DoctorId = reader.GetInt32("DoctorId"),
                                    StartTime = reader.GetDateTime("StartTime"),
                                    EndTime = reader.GetDateTime("EndTime"),
                                    AppointmentId = reader.IsDBNull("AppointmentId") ? null : reader.GetInt32("AppointmentId"),
                                    DoctorName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}",
                                    AppointmentReason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason")
                                };

                                timeSlotDtos.Add(dto);
                            }
                        }
                    }
                }

                Console.WriteLine($"Returning {timeSlotDtos.Count} time slot DTOs");
                return Ok(timeSlotDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting time slots: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/timeslots/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeSlotDto>> GetTimeSlot(int id)
        {
            try
            {
                var timeSlot = await _context.TimeSlots
                    .Include(ts => ts.Doctor)
                    .Include(ts => ts.Appointment)
                    .FirstOrDefaultAsync(ts => ts.Id == id);

                if (timeSlot == null)
                {
                    return NotFound();
                }

                var timeSlotDto = new TimeSlotDto
                {
                    Id = timeSlot.Id,
                    DoctorId = timeSlot.DoctorId,
                    StartTime = timeSlot.StartTime,
                    EndTime = timeSlot.EndTime,
                    AppointmentId = timeSlot.AppointmentId,
                    DoctorName = $"{timeSlot.Doctor.FirstName} {timeSlot.Doctor.LastName}",
                    AppointmentReason = timeSlot.Appointment?.Reason
                };

                return Ok(timeSlotDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/timeslots - Creates TimeSlot WITHOUT Appointment
        [HttpPost]
        public async Task<ActionResult<TimeSlotDto>> CreateTimeSlot([FromBody] TimeSlotDto timeSlotDto)
        {
            try
            {
                // Remove validation for display-only properties that shouldn't be required for creation
                ModelState.Remove(nameof(TimeSlotDto.DoctorName));
                ModelState.Remove(nameof(TimeSlotDto.AppointmentReason));
                ModelState.Remove(nameof(TimeSlotDto.Date));
                ModelState.Remove(nameof(TimeSlotDto.StartTimeOnly));
                ModelState.Remove(nameof(TimeSlotDto.EndTimeOnly));

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                Console.WriteLine($"Creating time slot for doctor {timeSlotDto.DoctorId}");

                // Validate doctor exists and is a doctor (Role = 1)
                var doctor = await _context.Users.FirstOrDefaultAsync(u => u.Id == timeSlotDto.DoctorId && u.Role == 1);
                if (doctor == null)
                {
                    return BadRequest("Invalid doctor specified.");
                }

                // Validate start time is before end time
                if (timeSlotDto.StartTime >= timeSlotDto.EndTime)
                {
                    return BadRequest("Start time must be before end time.");
                }

                // Validate that the time slot is in the future
                if (timeSlotDto.StartTime <= DateTime.Now)
                {
                    return BadRequest("Time slot must be scheduled for a future date and time.");
                }

                // Validate time slot doesn't overlap with existing ones for the same doctor
                var overlappingSlot = await _context.TimeSlots
                    .Where(ts => ts.DoctorId == timeSlotDto.DoctorId)
                    .Where(ts => (timeSlotDto.StartTime < ts.EndTime && timeSlotDto.EndTime > ts.StartTime))
                    .FirstOrDefaultAsync();

                if (overlappingSlot != null)
                {
                    return BadRequest("Time slot overlaps with an existing slot.");
                }

                // Create TimeSlot WITHOUT creating an Appointment
                var timeSlot = new TimeSlot
                {
                    DoctorId = timeSlotDto.DoctorId,
                    StartTime = timeSlotDto.StartTime,
                    EndTime = timeSlotDto.EndTime
                };

                _context.TimeSlots.Add(timeSlot);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Created time slot with ID: {timeSlot.Id}");

                // Return the created time slot with populated doctor information
                var resultDto = new TimeSlotDto
                {
                    Id = timeSlot.Id,
                    DoctorId = timeSlot.DoctorId,
                    StartTime = timeSlot.StartTime,
                    EndTime = timeSlot.EndTime,
                    AppointmentId = null,
                    DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                    AppointmentReason = null
                };

                return CreatedAtAction(nameof(GetTimeSlot), new { id = timeSlot.Id }, resultDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating time slot: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/timeslots/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeSlot(int id, [FromBody] TimeSlotDto timeSlotDto)
        {
            // Remove validation for display-only properties
            ModelState.Remove(nameof(TimeSlotDto.DoctorName));
            ModelState.Remove(nameof(TimeSlotDto.AppointmentReason));
            ModelState.Remove(nameof(TimeSlotDto.Date));
            ModelState.Remove(nameof(TimeSlotDto.StartTimeOnly));
            ModelState.Remove(nameof(TimeSlotDto.EndTimeOnly));

            if (id != timeSlotDto.Id)
            {
                return BadRequest("ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null)
            {
                return NotFound();
            }

            // Check if time slot is already booked with an appointment
            if (timeSlot.AppointmentId.HasValue)
            {
                return BadRequest("Cannot modify a time slot that has been booked with an appointment.");
            }

            // Validate start time is before end time
            if (timeSlotDto.StartTime >= timeSlotDto.EndTime)
            {
                return BadRequest("Start time must be before end time.");
            }

            // Validate time slot doesn't overlap with existing ones (excluding current slot)
            var overlappingSlot = await _context.TimeSlots
                .Where(ts => ts.DoctorId == timeSlotDto.DoctorId && ts.Id != id)
                .Where(ts => (timeSlotDto.StartTime < ts.EndTime && timeSlotDto.EndTime > ts.StartTime))
                .FirstOrDefaultAsync();

            if (overlappingSlot != null)
            {
                return BadRequest("Time slot overlaps with an existing slot.");
            }

            // Update only the time properties, don't modify AppointmentId
            timeSlot.StartTime = timeSlotDto.StartTime;
            timeSlot.EndTime = timeSlotDto.EndTime;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TimeSlotExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/timeslots/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            var timeSlot = await _context.TimeSlots.FindAsync(id);
            if (timeSlot == null)
            {
                return NotFound();
            }

            // Check if time slot is booked with an appointment
            if (timeSlot.AppointmentId.HasValue)
            {
                return BadRequest("Cannot delete a time slot that has been booked with an appointment.");
            }

            _context.TimeSlots.Remove(timeSlot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TimeSlotExists(int id)
        {
            return _context.TimeSlots.Any(e => e.Id == id);
        }
    }
}