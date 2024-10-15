using BrodClientAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrodClientAPI.Data;
using MongoDB.Driver;

namespace BrodClientAPI.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "AdminPolicy")]
    public class AdminController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public AdminController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.User.Find(user => true).ToListAsync(); // Fetch all users asynchronously from MongoDB
            return Ok(users);
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            user._id = "";
            await _context.User.InsertOneAsync(user); // Insert a new user asynchronously into the MongoDB collection
            return Ok(user);
        }

        [HttpGet("tradies")]
        public async Task<IActionResult> GetAllTradies()
        {
            var tradies = await _context.User.Find(user => user.Role == "Tradie").ToListAsync(); // Fetch all tradies asynchronously from MongoDB
            return Ok(tradies);
        }

        // Fetch tradie details by ID
        [HttpGet("tradie/")]
        public async Task<IActionResult> GetTradieById([FromBody] OwnProfile getTradieProfile)
        {
            var tradie = await _context.User.Find(user => user._id == getTradieProfile.ID && user.Role == "Tradie").FirstOrDefaultAsync();
            if (tradie == null)
            {
                return NotFound(new { message = "Tradie not found" });
            }
            return Ok(tradie);
        }

        [HttpPut("tradie/update-status")]
        public async Task<IActionResult> UpdateTradieStatus([FromBody] UpdateTradieStatus updateTradieStatus)
        {
            var tradie = await _context.User.Find(user => user._id == updateTradieStatus.ID && user.Role == "Tradie").FirstOrDefaultAsync();
            if (tradie == null)
            {
                return NotFound(new { message = "Tradie not found" });
            }

            // Update the status asynchronously
            var updateDefinition = Builders<User>.Update.Set(u => u.Status, updateTradieStatus.Status);
            await _context.User.UpdateOneAsync(user => user._id == updateTradieStatus.ID, updateDefinition);

            return Ok(new { message = "Tradie status updated successfully" });
        }
    }



}

