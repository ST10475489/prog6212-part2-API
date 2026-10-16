using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : ApiControllerBase
    {
        private readonly AppDbContext _db;

        public UsersController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>Gets the logged-in user's own profile.</summary>
        /// <remarks>Any role. The id in the route must be your own id. Returns the profile (never the password).</remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetUser(int id)
        {
            if (id != CurrentUserId()) return Forbid();

            var user = _db.Users.Find(id);
            if (user == null) return NotFound("User does not exist.");

            return Ok(ToDto(user));
        }

        /// <summary>Updates the logged-in user's own profile.</summary>
        /// <remarks>Expects fullName and email. Returns the updated profile.</remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateUser(int id, [FromBody] UpdateProfileRequest request)
        {
            if (id != CurrentUserId()) return Forbid();

            var user = _db.Users.Find(id);
            if (user == null) return NotFound("User does not exist.");

            if (request == null || string.IsNullOrWhiteSpace(request.FullName) || string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest("Full name and email are required.");
            }

            string email = request.Email.Trim().ToLower();
            if (_db.Users.Any(u => u.Email == email && u.UserId != id))
            {
                return BadRequest("Email is already in use.");
            }

            user.FullName = request.FullName.Trim();
            user.Email = email;
            _db.SaveChanges();

            return Ok(ToDto(user));
        }

        /// <summary>Gets the logged-in participant's full performance history.</summary>
        /// <remarks>Returns every result recorded against the user's enrolments.</remarks>
        [HttpGet("{id}/results")]
        [ProducesResponseType(typeof(List<ResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult GetUserResults(int id)
        {
            if (id != CurrentUserId()) return Forbid();

            var enrolments = _db.Enrolments.Where(e => e.ParticipantId == id).ToList();
            var builder = new ResultBuilder(_db);
            return Ok(builder.Build(enrolments));
        }

        private UserDto ToDto(Models.User user)
        {
            var dto = new UserDto();
            dto.UserId = user.UserId;
            dto.FullName = user.FullName;
            dto.Email = user.Email;
            dto.Role = user.Role;
            dto.CreatedAt = user.CreatedAt;
            return dto;
        }
    }
}
