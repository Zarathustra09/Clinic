using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Clinic.DataConnection;
using Clinic.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Clinic.Controllers
{
    [Route("Profile")]
    [Authorize]
    public class ProfileController : BaseController
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(SqlServerDbContext context, IWebHostEnvironment webHostEnvironment) 
            : base(context)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var profileDto = new ProfileDto
            {
                Id = user.Id,
                SchoolID = user.SchoolID,
                FirstName = user.FirstName,
                MiddleName = user.MiddleName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Program = user.Program,
                ProfileImage = user.ProfileImage,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return View(profileDto);
        }

        [HttpPost("Update")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProfileDto profileDto, IFormFile? profileImageFile, string? currentPassword, string? newPassword, string? confirmPassword)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int currentUserId))
            {
                return RedirectToAction("Index", "Login");
            }

            if (profileDto.Id != currentUserId)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (string.IsNullOrEmpty(currentPassword))
                {
                    ModelState.AddModelError("", "Current password is required to change password.");
                }
                else if (newPassword != confirmPassword)
                {
                    ModelState.AddModelError("", "New password and confirmation password do not match.");
                }
                else if (newPassword.Length < 6)
                {
                    ModelState.AddModelError("", "Password must be at least 6 characters long.");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var user = await _context.Users.FindAsync(profileDto.Id);
                    if (user == null)
                    {
                        return NotFound();
                    }

                    if (!string.IsNullOrEmpty(newPassword))
                    {
                        var hashedCurrentPassword = HashPassword(currentPassword);
                        if (user.PasswordHash != hashedCurrentPassword)
                        {
                            ModelState.AddModelError("", "Current password is incorrect.");
                            return View("Index", profileDto);
                        }
                        user.PasswordHash = HashPassword(newPassword);
                    }

                    if (profileImageFile != null && profileImageFile.Length > 0)
                    {
                        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
                        if (!allowedTypes.Contains(profileImageFile.ContentType.ToLower()))
                        {
                            TempData["ErrorMessage"] = "Please upload a valid image file (JPG, PNG, or GIF).";
                            return View("Index", profileDto);
                        }

                        if (profileImageFile.Length > 800 * 1024)
                        {
                            TempData["ErrorMessage"] = "Image file size must be less than 800KB.";
                            return View("Index", profileDto);
                        }

                        var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                        if (!Directory.Exists(uploadsPath))
                        {
                            Directory.CreateDirectory(uploadsPath);
                        }

                        var fileName = $"profile_{user.Id}_{Guid.NewGuid()}{Path.GetExtension(profileImageFile.FileName)}";
                        var filePath = Path.Combine(uploadsPath, fileName);

                        if (!string.IsNullOrEmpty(user.ProfileImage))
                        {
                            var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profileImageFile.CopyToAsync(stream);
                        }

                        user.ProfileImage = $"/uploads/profiles/{fileName}";
                    }

                    user.FirstName = profileDto.FirstName;
                    user.MiddleName = profileDto.MiddleName;
                    user.LastName = profileDto.LastName;
                    user.Email = profileDto.Email;
                    user.Username = profileDto.Username;
                    user.Program = profileDto.Program;
                    user.UpdatedAt = DateTime.UtcNow;

                    _context.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = !string.IsNullOrEmpty(newPassword) ?
                        "Profile and password updated successfully." :
                        "Profile updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "An error occurred while updating your profile.";
                }
            }

            return View("Index", profileDto);
        }

        [HttpPost("ResetProfileImage")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetProfileImage()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return RedirectToAction("Index", "Login");
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.ProfileImage))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfileImage.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }

                user.ProfileImage = null;
                user.UpdatedAt = DateTime.UtcNow;

                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Profile image removed successfully.";
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