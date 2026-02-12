using Domain.Enums;
using Infrasturcture.DataAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace User_Management_System.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly UserDbContext _db;

        public UsersController(UserDbContext db)
        {
            _db = db;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users
                .AsNoTracking()
                .OrderByDescending(x => x.LastLoginAt ?? DateTime.MinValue)
                .Select(x => new
                {
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.LastLoginAt,
                    x.RegisteredAt,
                    status = x.Status.ToString()
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("update")]
        public async Task<IActionResult> Put(UpdateUsersDto req)
        {
            if (req.Ids is null || req.Ids.Count == 0)
                return BadRequest("No users selected");

            var users = await _db.Users.Where(x => req.Ids.Contains(x.Id)).ToListAsync();
            if (users.Count == 0) return NotFound("Users not found");

            foreach (var u in users)
            {
                u.Status = req.Status;

                if (req.Status == UserStatus.Active && u.EmailConfirmedAt is null)
                {
                    u.EmailConfirmedAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { message = "Updated" });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromBody] DeleteUsersDto req)
        {
            if (req.Ids is null || req.Ids.Count == 0)
                return BadRequest("No users selected");

            var users = await _db.Users.Where(x => req.Ids.Contains(x.Id)).ToListAsync();
            _db.Users.RemoveRange(users);

            await _db.SaveChangesAsync();
            return Ok(new { message = "Deleted" });
        }

        [HttpDelete("delete/unverified")]
        public async Task<IActionResult> DeleteUnverified()
        {
            var users = await _db.Users.Where(x => x.Status == UserStatus.Unverified).ToListAsync();
            _db.Users.RemoveRange(users);

            await _db.SaveChangesAsync();
            return Ok(new { message = "Unverified users deleted", count = users.Count });
        }

    }
}
