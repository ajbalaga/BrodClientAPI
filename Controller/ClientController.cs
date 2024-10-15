using BrodClientAPI.Data;
using BrodClientAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;

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

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] User clientProfile)
        {
            try
            {
                var client = await _context.User.Find(user => user._id == clientProfile._id && user.Role == "Client").FirstOrDefaultAsync();
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

                var updateDefinition = Builders<User>.Update.Combine(updateDefinitions);
                var filter = Builders<User>.Filter.Eq(u => u._id, clientProfile._id);

                await _context.User.UpdateOneAsync(filter, updateDefinition);

                return Ok(new { message = "Client profile updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile", error = ex.Message });
            }
        }

        [HttpPost("AddReviewToJobPost")]
        public async Task<IActionResult> AddReviewToJobPost([FromBody] AddReviewToJobPostAd reviewDetails)
        {
            try
            {
                var client = await _context.User.Find(user => user._id == reviewDetails.ClientID && user.Role == "Client").FirstOrDefaultAsync();
                if (client == null)
                {
                    return NotFound(new { message = "Client not found" });
                }

                var existingService = await _context.Services.Find(service => service._id == reviewDetails.ServiceID).FirstOrDefaultAsync();
                if (existingService == null)
                {
                    return NotFound(new { message = "Job Post Ad not found" });
                }

                var review = new Reviews
                {
                    _id = "",
                    ServiceID = reviewDetails.ServiceID,
                    ClientID = reviewDetails.ClientID,
                    ClientUserName = client.Username,
                    ClientCity = client.City,
                    ClientState = client.State,
                    ClientPostalCode = client.PostalCode,
                    StarRating = reviewDetails.StarRating,
                    ReviewDescription = reviewDetails.ReviewDescription
                };

                await _context.Reviews.InsertOneAsync(review);

                // Prepare the update to append the review to the ClientReviews list in the Service
                var update = Builders<Services>.Update.Push(s => s.ClientReviews, new Review
                {
                    ReviewDescription = reviewDetails.ReviewDescription,
                    StarRating = reviewDetails.StarRating,
                    ClientID = reviewDetails.ClientID,
                    ClientUserName = client.Username,
                    ClientCity = client.City,
                    ClientState = client.State,
                    ClientPostalCode = client.PostalCode
                });

                // Update the service with the new review
                await _context.Services.UpdateOneAsync(service => service._id == reviewDetails.ServiceID, update);

                return Ok(new { message = "Review post added successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding review to job post", error = ex.Message });
            }
        }

        [HttpGet("GetJobsByStatus")]
        public async Task<IActionResult> GetFilteredJobs([FromBody] GetJobsByStatus jobsByStatus)
        {
            try
            {
                var client = await _context.User
                    .Find(user => user._id == jobsByStatus.UserID && user.Role.ToLower() == "client")
                    .FirstOrDefaultAsync();

                if (client == null)
                {
                    return NotFound(new { message = "Client not found" });
                }

                var jobFilterBuilder = Builders<Jobs>.Filter;
                var jobFilter = jobFilterBuilder.Eq(job => job.Status.ToLower().Replace(" ", ""), jobsByStatus.Status.ToLower().Replace(" ", "")) &
                                jobFilterBuilder.Eq(job => job.ClientID, jobsByStatus.UserID);

                var jobs = await _context.Jobs.Find(jobFilter).ToListAsync();
                if (jobs.Count < 1)
                {
                    return NotFound(new { message = "Job/s not found" });
                }

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while getting job post", error = ex.Message });
            }
        }

        [HttpPut("UpdateJobStatus")]
        public async Task<IActionResult> UpdateJobStatus([FromBody] UpdateJobStatus updateJobStatus)
        {
            try
            {
                var job = await _context.Jobs.Find(job => job._id == updateJobStatus.JobID).FirstOrDefaultAsync();
                if (job == null)
                {
                    return NotFound(new { message = "Job not found" });
                }

                // Update the status
                var updateDefinition = Builders<Jobs>.Update.Set(u => u.Status, updateJobStatus.Status);
                await _context.Jobs.UpdateOneAsync(user => user._id == updateJobStatus.JobID, updateDefinition);

                var tradie = await _context.User.Find(user => user._id == updateJobStatus.TradieID && user.Role.ToLower() == "tradie").FirstOrDefaultAsync();

                if (updateJobStatus.Status.ToLower() == "cancelled")
                {
                    var jobCount = tradie.PendingOffers == 0 ? 0 : tradie.PendingOffers - 1;
                    var countVal = new UpdateCount { TradieID = updateJobStatus.TradieID, Count = jobCount };
                    await UpdateJobOfferCount(countVal);
                }

                if (updateJobStatus.Status.ToLower() == "completed")
                {
                    var addCountJobCompleted = new UpdateCount { TradieID = updateJobStatus.TradieID, Count = tradie.CompletedJobs + 1 };
                    await UpdateCompletetedJobs(addCountJobCompleted);

                    var earningAmount = Convert.ToDecimal(tradie.EstimatedEarnings) + Convert.ToDecimal(job.ClientBudget);
                    var addEarning = new UpdateEstimatedEarning { TradieID = updateJobStatus.TradieID, Earning = earningAmount };
                    await UpdateEstimatedEarningOfTradie(addEarning);
                }

                return Ok(new { message = "Job status updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the job status", error = ex.Message });
            }
        }

        // Add async to this method as well
        private async Task<IActionResult> UpdateJobOfferCount([FromBody] UpdateCount updateCount)
        {
            try
            {
                var tradie = await _context.User.Find(user => user._id == updateCount.TradieID && user.Role == "Tradie").FirstOrDefaultAsync();
                if (tradie == null)
                {
                    return NotFound(new { message = "Tradie not found" });
                }

                // Update the status
                var updateDefinition = Builders<User>.Update.Set(u => u.PendingOffers, updateCount.Count);
                await _context.User.UpdateOneAsync(user => user._id == updateCount.TradieID, updateDefinition);
                return Ok(new { message = "Job offer count updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating job offer count", error = ex.Message });
            }
        }

        private async Task<IActionResult> UpdateEstimatedEarningOfTradie([FromBody] UpdateEstimatedEarning updateEarning)
        {
            try
            {
                var tradie = await _context.User.Find(user => user._id == updateEarning.TradieID && user.Role == "Tradie").FirstOrDefaultAsync();
                if (tradie == null)
                {
                    return NotFound(new { message = "Tradie not found" });
                }

                // Update the status
                var updateDefinition = Builders<User>.Update.Set(u => u.EstimatedEarnings, updateEarning.Earning);
                await _context.User.UpdateOneAsync(user => user._id == updateEarning.TradieID, updateDefinition);
                return Ok(new { message = "Tradie earnings updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating tradie earnings", error = ex.Message });
            }
        }

        private async Task<IActionResult> UpdateCompletetedJobs([FromBody] UpdateCount updateCount)
        {
            try
            {
                var tradie = await _context.User.Find(user => user._id == updateCount.TradieID && user.Role == "Tradie").FirstOrDefaultAsync();
                if (tradie == null)
                {
                    return NotFound(new { message = "Tradie not found" });
                }

                // Update the status
                var updateDefinition = Builders<User>.Update.Set(u => u.CompletedJobs, updateCount.Count);
                await _context.User.UpdateOneAsync(user => user._id == updateCount.TradieID, updateDefinition);
                return Ok(new { message = "Tradie completed jobs updated successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating tradie completed jobs", error = ex.Message });
            }
        }
    }

}
