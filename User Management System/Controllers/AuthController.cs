using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrasturcture.DataAccess;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace User_Management_System.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly UserDbContext _db;
        private readonly IJwtTokenService _jwt;
        private readonly IEmailSender _email;

        public AuthController(UserDbContext db, IJwtTokenService jwt, IEmailSender email)
        {
            _db = db;
            _jwt = jwt;
            _email = email;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(Register register)
        {
            var now = DateTime.UtcNow;

            var user = new User
            {
                FullName = register.FullName,
                Email = register.Email.Trim().ToLowerInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(register.Password),
                Status = UserStatus.Unverified,
                RegisteredAt = now,
                VerficationToken = Guid.NewGuid().ToString("N"),
                VerficationTokenExpiredAt = now.AddHours(24)
            };

            await _db.Users.AddAsync(user);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException pg && pg.SqlState == "23505")
            {
                return Conflict("This email already exists!");
            }

            var confirmLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?token={user.VerficationToken}";

            try
            {
                await _email.SendConfirmEmailAsync(user.Email, confirmLink);
            }
            catch (Exception ex)
            {
            }

            return Ok(new
            {
                message = "Registered. Confirmation email sent",
            });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(Login login)
        {
            var email = login.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user is null)
            {
                return Unauthorized("Email or Password is incorrect!");
            }

            if (user.Status == UserStatus.Blocked)
            {
                return StatusCode(403, "User blocked");
            }


            var ok = BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash);
            if (!ok) return Unauthorized("Email or Password is incorrect");
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            var token = _jwt.CreateToken(user);
            return Ok(new { token });

        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest("Token Required");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.VerficationToken == token);
            if (user is null) return NotFound("Invalid Token");

            if (user.VerficationTokenExpiredAt is not null && user.VerficationTokenExpiredAt < DateTime.UtcNow)
                return BadRequest("Token Expired");

            if (user.Status == UserStatus.Unverified)
            {
                user.Status = UserStatus.Active;
                user.EmailConfirmedAt = DateTime.UtcNow;
            }

            user.VerficationToken = null;
            user.VerficationTokenExpiredAt = null;

            await _db.SaveChangesAsync();
            return Ok("Email confirmed");
        }
    }
}
