using BrodClientAPI.Data;
using BrodClientAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BrodClientAPI.Controller
{
    [Authorize(Policy = "ClientPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public ClientController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("myDetails")]
        public IActionResult GetClientById([FromBody] OwnProfile getTradieProfile)
        {
            var tradie = _context.User.Find(user => user._id == getTradieProfile.ID && user.Role == "Client").FirstOrDefault();
            if (tradie == null)
            {
                return NotFound(new { message = "Client not found" });
            }
            return Ok(tradie);
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
            if (!string.IsNullOrEmpty(clientProfile.Username) && client.Username != clientProfile.Username)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Username, clientProfile.Username));
            }
            if (!string.IsNullOrEmpty(clientProfile.FirstName) && client.FirstName != clientProfile.FirstName)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.FirstName, clientProfile.FirstName));
            }
            if (!string.IsNullOrEmpty(clientProfile.LastName) && client.LastName != clientProfile.LastName)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.LastName, clientProfile.LastName));
            }
            if (!string.IsNullOrEmpty(clientProfile.Username) && client.Username != clientProfile.Username)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Username, clientProfile.Username));
            }
            if (!string.IsNullOrEmpty(clientProfile.Email) && client.Email != clientProfile.Email)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Email, clientProfile.Email));
            }
            if (!string.IsNullOrEmpty(clientProfile.ContactNumber) && client.ContactNumber != clientProfile.ContactNumber)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ContactNumber, clientProfile.ContactNumber));
            }
            if (!string.IsNullOrEmpty(clientProfile.State) && client.State != clientProfile.State)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.State, clientProfile.State));
            }
            if (!string.IsNullOrEmpty(clientProfile.City) && client.City != clientProfile.City)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.City, clientProfile.City));
            }
            if (!string.IsNullOrEmpty(clientProfile.PostalCode) && client.PostalCode != clientProfile.PostalCode)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.PostalCode, clientProfile.PostalCode));
            }

            // Update Profile Picture if provided
            if (!string.IsNullOrEmpty(clientProfile.ProfilePicture) && client.ProfilePicture != clientProfile.ProfilePicture)
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

            return Ok(new { message = "Client profile updated successfully" });
        }


        [HttpGet("allServices")]
        public IActionResult GetAllServices()
        {
            var services = _context.Services.Find(services => true).ToList(); // Fetch all users from MongoDB
            return Ok(services);
        }
    }
}
