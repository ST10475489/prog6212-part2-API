using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Data;
using RaceDay.Api.Dtos;
using RaceDay.Api.Models;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TokenService _tokens;

        public AuthController(AppDbContext db, TokenService tokens)
        {
            _db = db;
            _tokens = tokens;
        }

        /// <summary>Registers a new Organiser or Participant account.</summary>
        /// <remarks>Expects fullName, email, password (min 8 characters) and role ("Organiser" or "Participant"). The password is hashed before it is stored. Returns the new user id and role.</remarks>
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Full name, email and password are required.");
            }

            if (request.Password.Length < 8)
            {
                return BadRequest("Password must be at least 8 characters.");
            }

            string role = Roles.Normalise(request.Role);
            if (role == null)
            {
                return BadRequest("Role must be Organiser or Participant.");
            }

            string email = request.Email.Trim().ToLower();
            if (_db.Users.Any(u => u.Email == email))
            {
                return BadRequest("Email is already registered.");
            }

            var user = new User();
            user.FullName = request.FullName.Trim();
            user.Email = email;
            user.Role = role;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.CreatedAt = DateTime.UtcNow;

            _db.Users.Add(user);
            _db.SaveChanges();

            return StatusCode(201, new { userId = user.UserId, role = user.Role });
        }

        /// <summary>Logs a user in.</summary>
        /// <remarks>Expects email and password. Returns a JWT token and the user's role. Send the token as "Authorization: Bearer {token}" on later requests.</remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            string email = request.Email.Trim().ToLower();
            var user = _db.Users.FirstOrDefault(u => u.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("Invalid credentials.");
            }

            var response = new LoginResponse();
            response.Token = _tokens.CreateToken(user);
            response.Role = user.Role;
            response.UserId = user.UserId;
            return Ok(response);
        }
    }
}
