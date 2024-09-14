using BrodClientAPI.Data;
using BrodClientAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
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

        [HttpGet("FilteredServices")]
        public IActionResult GetFilteredServices([FromBody] JobAdPostFilter filterInput)
        {
            try
            {
                var filterBuilder = Builders<Services>.Filter;
                var filter = filterBuilder.Empty; // Start with an empty filter

                // Filter by Postcode
                if (!string.IsNullOrEmpty(filterInput.Postcode))
                {
                    filter &= filterBuilder.Eq(s => s.BusinessPostcode, filterInput.Postcode);
                }

                // Filter by JobCategory (if multiple categories are provided)
                if (filterInput.JobCategories != null && filterInput.JobCategories.Count > 0)
                {
                    filter &= filterBuilder.In(s => s.JobCategory, filterInput.JobCategories);
                }

                // Filter by Keywords (match JobAdTitle using a case-insensitive regex)
                if (!string.IsNullOrEmpty(filterInput.Keywords))
                {
                    var regexFilter = new BsonRegularExpression(filterInput.Keywords, "i"); // Case-insensitive search
                    filter &= filterBuilder.Regex(s => s.JobAdTitle, regexFilter);
                }


                // Filter by PricingStartsAt (range between min and max)
                if (filterInput.PricingStartsMax> filterInput.PricingStartsMin)
                {
                    filter &= filterBuilder.Eq(s => s.PricingOption, "Hourly");
                    filter &= filterBuilder.Gte(s => s.PricingStartsAt, filterInput.PricingStartsMin.ToString()) &
                              filterBuilder.Lte(s => s.PricingStartsAt, filterInput.PricingStartsMax.ToString());
                }

                var filteredServices = _context.Services.Find(filter).ToList();

                var userIds = filteredServices.Select(s => s.UserID).Distinct().ToList();


                var userFilterBuilder = Builders<User>.Filter;
                var userFilter = userFilterBuilder.In(u => u._id, userIds);

                if (filterInput.CallOutRateMax > filterInput.CallOutRateMin)
                {
                    userFilter &= userFilterBuilder.Gte(u => u.CallOutRate, filterInput.CallOutRateMin.Value.ToString()) &
                                  userFilterBuilder.Lte(u => u.CallOutRate, filterInput.CallOutRateMin.Value.ToString());
                }

                // Filter by ProximityToWork (min and max range)
                if (filterInput.ProximityToWorkMax > filterInput.ProximityToWorkMin)
                {
                    userFilter &= userFilterBuilder.Gte(u => u.ProximityToWork, filterInput.ProximityToWorkMin.Value.ToString()) &
                                  userFilterBuilder.Lte(u => u.ProximityToWork, filterInput.ProximityToWorkMax.Value.ToString());
                }

                // Filter by AvailabilityToWork (multiple answers)
                if (!String.IsNullOrEmpty(filterInput.AvailabilityToWork[0]) && filterInput.AvailabilityToWork.Count > 0)
                {
                    userFilter &= userFilterBuilder.In(u => u.AvailabilityToWork, filterInput.AvailabilityToWork);
                }

                // Fetch the filtered users that match the user filters
                var filteredUsers = _context.User.Find(userFilter).ToList();
                var finalUserIds = filteredUsers.Select(u => u._id).ToList();

                // Step 4: Filter the services again based on the final list of UserIDs from the User filter
                var finalServices = filteredServices.Where(s => finalUserIds.Contains(s.UserID)).ToList();

                return Ok(finalServices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving services", error = ex.Message });
            }
        }
        [HttpGet("JobPostDetails")]
        public IActionResult GetJobPostDetails([FromBody] OwnProfile serviceProfile)
        {
            var service = _context.Services.Find(service => service._id == serviceProfile.ID).FirstOrDefault();
            if (service == null)
            {
                return NotFound(new { message = "Job Ad Post not found" });
            }
            return Ok(service);
        }



    }
}
