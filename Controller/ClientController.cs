using BrodClientAPI.Data;
using BrodClientAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BrodClientAPI.Controller
{
    [Authorize(Policy = "UserPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ClientController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var username = User.Identity.Name;
            var user = _context.User.Find(u => u.Username == username).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPut("update-profile")]
        public IActionResult UpdateProfile([FromBody] User user)
        {
            var existingUser = _context.User.Find(u => u.Username == User.Identity.Name).FirstOrDefault();
            if (existingUser == null)
            {
                return NotFound();
            }

            var update = Builders<User>.Update.Set(u => u.Email, user.Email);
            _context.User.UpdateOne(u => u.Username == existingUser.Username, update);

            return Ok(existingUser);
        }
    }
}
