using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Services;
using System.Threading.Tasks;

namespace RoleplayersGuild.Site.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Ensure only logged-in users can access this
    [EnableCors("AllowViteDevServer")]
    public class UserApiController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserApiController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("activity")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateActivity()
        {
            var userId = _userService.GetUserId(User);
            if (userId == 0)
            {
                return Unauthorized();
            }

            await _userService.UpdateUserLastActionAsync(userId);
            return Ok();
        }
    }
}