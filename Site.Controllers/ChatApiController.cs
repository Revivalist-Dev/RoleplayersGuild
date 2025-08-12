using Microsoft.AspNetCore.Authorization; // Add this using statement
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RoleplayersGuild.Site.Model;
using RoleplayersGuild.Site.Services;
using RoleplayersGuild.Site.Services.DataServices;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace RoleplayersGuild.Site.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatApiController : ControllerBase
    {
        private readonly ICommunityDataService _communityDataService;
        private readonly IUserService _userService;
        private readonly IPassCryptService _passCryptService;
        private readonly IJwtService _jwtService;

        public ChatApiController(
            ICommunityDataService communityDataService,
            IUserService userService,
            IPassCryptService passCryptService,
            IJwtService jwtService)
        {
            _communityDataService = communityDataService;
            _userService = userService;
            _passCryptService = passCryptService;
            _jwtService = jwtService;
        }

        [HttpPost("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginInputModel model)
        {
            Console.WriteLine("--- Login Endpoint Hit ---");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("Login failed: Model state is invalid.");
                return BadRequest(ModelState);
            }

            Console.WriteLine($"Attempting to find user by email: {model.Email}");
            var user = await _userService.GetUserByEmailAsync(model.Email);

            if (user == null)
            {
                Console.WriteLine("Login failed: User not found.");
                return Unauthorized(new { message = "Invalid credentials." });
            }

            Console.WriteLine($"User found: {user.Username}. Verifying password...");
            var isPasswordValid = _passCryptService.VerifyPassword(user.Password, model.Password);

            if (isPasswordValid == PasswordVerificationResult.Failed)
            {
                Console.WriteLine("Login failed: Password verification failed.");
                return Unauthorized(new { message = "Invalid credentials." });
            }

            Console.WriteLine("Password verified. Generating JWT...");
            var token = _jwtService.GenerateToken(user);

            Console.WriteLine("JWT generated. Returning success.");
            Console.WriteLine("--------------------------");
            return Ok(new { token });
        }

        [HttpGet("GetPosts")]
        public async Task<ActionResult<IEnumerable<ChatRoomPostsWithDetails>>> GetChatPosts(int chatRoomId, int lastPostId = 0)
        {
            var posts = await _communityDataService.GetChatRoomPostsAsync(chatRoomId, lastPostId);
            return Ok(posts);
        }

        [HttpGet("GetActiveRooms")]
        public async Task<ActionResult<IEnumerable<DashboardChatRoom>>> GetActiveRooms()
        {
            var userId = _userService.GetUserId(User);
            var rooms = await _communityDataService.GetActiveChatRoomsForDashboardAsync(userId);
            return Ok(rooms);
        }
        [HttpGet("GetCharacters")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<CharacterSimpleViewModel>>> GetCharacters()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized();
            }

            var characters = await _communityDataService.GetRecordsAsync<CharacterSimpleViewModel>(
                @"SELECT ""CharacterId"", ""CharacterDisplayName"" FROM ""Characters"" WHERE ""UserId"" = @UserId ORDER BY ""CharacterDisplayName""",
                new { UserId = userId }
            );

            return Ok(characters);
        }
    }
}
