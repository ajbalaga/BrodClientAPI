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
        public IActionResult UpdateProfile([FromBody] User clientProfile)
        {
            var client = _context.User.Find(user => user._id == clientProfile._id && user.Role == "Client").FirstOrDefault();
            if (client == null)
            {
                return NotFound(new { message = "Client not found" });
            }

            var updateDefinitions = new List<UpdateDefinition<User>>();

            // Update fields only if they are provided in clientProfile
            if (!string.IsNullOrEmpty(clientProfile.Username) && clientProfile.Username != clientProfile.Username)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Username, clientProfile.Username));
            }
            if (!string.IsNullOrEmpty(clientProfile.Email) && clientProfile.Email != clientProfile.Email)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Email, clientProfile.Email));
            }
            if (!string.IsNullOrEmpty(clientProfile.ContactNumber) && clientProfile.ContactNumber != clientProfile.ContactNumber)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ContactNumber, clientProfile.ContactNumber));
            }
            if (!string.IsNullOrEmpty(clientProfile.State) && clientProfile.State != clientProfile.State)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.State, clientProfile.State));
            }
            if (!string.IsNullOrEmpty(clientProfile.City) && clientProfile.City != clientProfile.City)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.City, clientProfile.City));
            }
            if (!string.IsNullOrEmpty(clientProfile.PostalCode) && clientProfile.PostalCode != clientProfile.PostalCode)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.PostalCode, clientProfile.PostalCode));
            }

            // Update Profile Picture if provided
            if (!string.IsNullOrEmpty(clientProfile.ProfilePicture) && clientProfile.ProfilePicture != clientProfile.ProfilePicture)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProfilePicture, clientProfile.ProfilePicture));
            }

            if (updateDefinitions.Count == 0)
            {
                return BadRequest(new { message = "No valid fields to update" });
            }

            try
            {
                var updateDefinition = Builders<User>.Update.Combine(updateDefinitions);
                var filter = Builders<User>.Filter.Eq(u => u._id, clientProfile._id);

                _context.User.UpdateOne(filter, updateDefinition);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile", error = ex.Message });
            }

            return Ok(new { message = "Tradie profile updated successfully" });
        }
    }
}
