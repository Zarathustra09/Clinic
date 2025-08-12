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

        // GET: api/timeslots/{id} - FIXED VERSION with Raw SQL
        [HttpGet("{id}")]
        public async Task<ActionResult<TimeSlotDto>> GetTimeSlot(int id)
        {
            try
            {
                Console.WriteLine($"Getting time slot with ID: {id}");

                var connectionString = _context.Database.GetConnectionString();
                TimeSlotDto timeSlotDto = null;

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var sql = @"
                        SELECT ts.Id, ts.DoctorId, ts.StartTime, ts.EndTime, ts.AppointmentId,
                               u.FirstName, u.LastName, a.Reason
                        FROM [Clinic].[TimeSlot] ts
                        INNER JOIN [Clinic].[User] u ON ts.DoctorId = u.Id
                        LEFT JOIN [Clinic].[Appointment] a ON ts.AppointmentId = a.Id
                        WHERE ts.Id = @Id 
                        AND ts.Id IS NOT NULL 
                        AND ts.DoctorId IS NOT NULL 
                        AND ts.StartTime IS NOT NULL 
                        AND ts.EndTime IS NOT NULL";

                    using (var command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                timeSlotDto = new TimeSlotDto
                                {
                                    Id = reader.GetInt32("Id"),
                                    DoctorId = reader.GetInt32("DoctorId"),
                                    StartTime = reader.GetDateTime("StartTime"),
                                    EndTime = reader.GetDateTime("EndTime"),
                                    AppointmentId = reader.IsDBNull("AppointmentId") ? null : reader.GetInt32("AppointmentId"),
                                    DoctorName = $"{reader.GetString("FirstName")} {reader.GetString("LastName")}",
                                    AppointmentReason = reader.IsDBNull("Reason") ? null : reader.GetString("Reason")
                                };
                            }
                        }
                    }
                }

                if (timeSlotDto == null)
                {
                    Console.WriteLine($"Time slot with ID {id} not found");
                    return NotFound();
                }

                Console.WriteLine($"Found time slot: {timeSlotDto.Id} - {timeSlotDto.StartTime} to {timeSlotDto.EndTime}");
                return Ok(timeSlotDto);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting time slot: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

       [HttpPost]
       public async Task<ActionResult<TimeSlotDto>> CreateTimeSlot([FromBody] TimeSlotDto timeSlotDto)
       {
           try
           {
               // Remove validation for display-only properties
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
       
               // Use a database transaction to prevent duplicates
               using var transaction = await _context.Database.BeginTransactionAsync();
               
               try
               {
                   // Check for exact duplicate (same doctor, same start time, same end time)
                   var exactDuplicate = await _context.TimeSlots
                       .FirstOrDefaultAsync(ts => ts.DoctorId == timeSlotDto.DoctorId 
                                               && ts.StartTime == timeSlotDto.StartTime 
                                               && ts.EndTime == timeSlotDto.EndTime);
       
                   if (exactDuplicate != null)
                   {
                       return BadRequest("A time slot with the exact same time already exists for this doctor.");
                   }
       
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
       
                   // Check for overlapping slots using Entity Framework
                   var hasOverlap = await _context.TimeSlots
                       .AnyAsync(ts => ts.DoctorId == timeSlotDto.DoctorId 
                                   && ts.StartTime < timeSlotDto.EndTime 
                                   && ts.EndTime > timeSlotDto.StartTime);
       
                   if (hasOverlap)
                   {
                       return BadRequest("Time slot overlaps with an existing slot.");
                   }
       
                   // Create TimeSlot
                   var timeSlot = new TimeSlot
                   {
                       DoctorId = timeSlotDto.DoctorId,
                       StartTime = timeSlotDto.StartTime,
                       EndTime = timeSlotDto.EndTime,
                       AppointmentId = null
                   };
       
                   _context.TimeSlots.Add(timeSlot);
                   await _context.SaveChangesAsync();
                   
                   // Commit the transaction
                   await transaction.CommitAsync();
       
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
               catch (Exception)
               {
                   await transaction.RollbackAsync();
                   throw;
               }
           }
           catch (Exception ex)
           {
               Console.WriteLine($"Error creating time slot: {ex.Message}");
               return StatusCode(500, $"Internal server error: {ex.Message}");
           }
       }

        // PUT: api/timeslots/{id} - FIXED VERSION with Raw SQL
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTimeSlot(int id, [FromBody] TimeSlotDto timeSlotDto)
        {
            try
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

                Console.WriteLine($"Updating time slot with ID: {id}");

                var connectionString = _context.Database.GetConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // First check if the time slot exists and get its current state
                    var checkSql = @"
                        SELECT AppointmentId 
                        FROM [Clinic].[TimeSlot] 
                        WHERE Id = @Id";

                    int? appointmentId = null;
                    bool timeSlotExists = false;

                    using (var command = new SqlCommand(checkSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                timeSlotExists = true;
                                appointmentId = reader.IsDBNull("AppointmentId") ? null : reader.GetInt32("AppointmentId");
                            }
                        }
                    }

                    if (!timeSlotExists)
                    {
                        return NotFound();
                    }

                    // Check if time slot is already booked with an appointment
                    if (appointmentId.HasValue)
                    {
                        return BadRequest("Cannot modify a time slot that has been booked with an appointment.");
                    }

                    // Validate start time is before end time
                    if (timeSlotDto.StartTime >= timeSlotDto.EndTime)
                    {
                        return BadRequest("Start time must be before end time.");
                    }

                    // Check for overlapping slots (excluding current slot)
                    var overlapSql = @"
                        SELECT COUNT(*) 
                        FROM [Clinic].[TimeSlot] 
                        WHERE DoctorId = @DoctorId 
                        AND Id != @Id
                        AND (@StartTime < EndTime AND @EndTime > StartTime)";

                    using (var command = new SqlCommand(overlapSql, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorId", timeSlotDto.DoctorId);
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@StartTime", timeSlotDto.StartTime);
                        command.Parameters.AddWithValue("@EndTime", timeSlotDto.EndTime);

                        var count = (int)await command.ExecuteScalarAsync();
                        if (count > 0)
                        {
                            return BadRequest("Time slot overlaps with an existing slot.");
                        }
                    }

                    // Update the time slot
                    var updateSql = @"
                        UPDATE [Clinic].[TimeSlot] 
                        SET StartTime = @StartTime, EndTime = @EndTime 
                        WHERE Id = @Id";

                    using (var command = new SqlCommand(updateSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);
                        command.Parameters.AddWithValue("@StartTime", timeSlotDto.StartTime);
                        command.Parameters.AddWithValue("@EndTime", timeSlotDto.EndTime);

                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            return NotFound();
                        }
                    }
                }

                Console.WriteLine($"Successfully updated time slot {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating time slot: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/timeslots/{id} - FIXED VERSION with Raw SQL
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTimeSlot(int id)
        {
            try
            {
                Console.WriteLine($"Deleting time slot with ID: {id}");

                var connectionString = _context.Database.GetConnectionString();

                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // First check if the time slot exists and get its current state
                    var checkSql = @"
                        SELECT AppointmentId 
                        FROM [Clinic].[TimeSlot] 
                        WHERE Id = @Id";

                    int? appointmentId = null;
                    bool timeSlotExists = false;

                    using (var command = new SqlCommand(checkSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                timeSlotExists = true;
                                appointmentId = reader.IsDBNull("AppointmentId") ? null : reader.GetInt32("AppointmentId");
                            }
                        }
                    }

                    if (!timeSlotExists)
                    {
                        return NotFound();
                    }

                    // Check if time slot is booked with an appointment
                    if (appointmentId.HasValue)
                    {
                        return BadRequest("Cannot delete a time slot that has been booked with an appointment.");
                    }

                    // Delete the time slot
                    var deleteSql = @"DELETE FROM [Clinic].[TimeSlot] WHERE Id = @Id";

                    using (var command = new SqlCommand(deleteSql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        if (rowsAffected == 0)
                        {
                            return NotFound();
                        }
                    }
                }

                Console.WriteLine($"Successfully deleted time slot {id}");
                return NoContent();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting time slot: {ex.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private bool TimeSlotExists(int id)
        {
            return _context.TimeSlots.Any(e => e.Id == id);
        }
    }
}