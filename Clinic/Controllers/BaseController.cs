using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Clinic.DataConnection;

namespace Clinic.Controllers
{
    public class BaseController : Controller
    {
        protected readonly SqlServerDbContext _context;

        public BaseController(SqlServerDbContext context)
        {
            _context = context;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    var user = await _context.Users.FindAsync(userId);
                    ViewBag.CurrentUserProfileImage = user?.ProfileImage;
                }
            }

            await next();
        }
    }
}