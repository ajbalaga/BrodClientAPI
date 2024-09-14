using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BrodClientAPI.Data;
using BrodClientAPI.Models;
using MongoDB.Driver;
using System.Diagnostics;

namespace BrodClientAPI.Controller
{
    [Authorize(Policy = "TradiePolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class TradieController : ControllerBase
    {
        private readonly ApiDbContext _context;
        public TradieController(ApiDbContext context)
        {
            _context = context;
        }

        [HttpGet("tasks")]
        public IActionResult GetTasks()
        {
            // Here you would return tasks related to the logged-in employee
            var username = User.Identity.Name;
            // Fetch tasks from the database based on the employee's username or ID
            return Ok(new { message = "Here are the tasks for employee " + username });
        }

        [HttpPost("update-task-status")]
        public IActionResult UpdateTaskStatus([FromBody] string taskStatus)
        {
            // Logic to update task status
            return Ok(new { message = "Task status updated" });
        }

        [HttpGet("myDetails")]
        public IActionResult GetTradieById([FromBody] GetTradieProfile getTradieProfile)
        {
            var tradie = _context.User.Find(user => user._id == getTradieProfile.ID && user.Role == "Tradie").FirstOrDefault();
            if (tradie == null)
            {
                return NotFound(new { message = "Tradie not found" });
            }
            return Ok(tradie);
        }

        [HttpPut("update-tradie-profile")]
        public IActionResult UpdateTradieProfile([FromBody] UpdateUserProfile tradieProfile)
        {
            var tradie = _context.User.Find(user => user._id == tradieProfile.ID && user.Role == "Tradie").FirstOrDefault();
            if (tradie == null)
            {
                return NotFound(new { message = "Tradie not found" });
            }

            var updateDefinitions = new List<UpdateDefinition<User>>();

            // Update fields only if they are provided in tradieProfile
            if (!string.IsNullOrEmpty(tradieProfile.AvailabilityToWork) && tradieProfile.AvailabilityToWork != tradie.AvailabilityToWork)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.AvailabilityToWork, tradieProfile.AvailabilityToWork));
            }
            if (!string.IsNullOrEmpty(tradieProfile.Email) && tradieProfile.Email != tradie.Email)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Email, tradieProfile.Email));
            }
            if (!string.IsNullOrEmpty(tradieProfile.Website) && tradieProfile.Website != tradie.Website)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.Website, tradieProfile.Website));
            }
            if (!string.IsNullOrEmpty(tradieProfile.FacebookAccount) && tradieProfile.FacebookAccount != tradie.FacebookAccount)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.FacebookAccount, tradieProfile.FacebookAccount));
            }
            if (!string.IsNullOrEmpty(tradieProfile.IGAccount) && tradieProfile.IGAccount != tradie.IGAccount)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.IGAccount, tradieProfile.IGAccount));
            }
            if (!string.IsNullOrEmpty(tradieProfile.AboutMeDescription) && tradieProfile.AboutMeDescription != tradie.AboutMeDescription)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.AboutMeDescription, tradieProfile.AboutMeDescription));
            }
            if (!string.IsNullOrEmpty(tradieProfile.ProximityToWork) && tradieProfile.ProximityToWork != tradie.ProximityToWork)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProximityToWork, tradieProfile.ProximityToWork));
            }
            if (!string.IsNullOrEmpty(tradieProfile.City) && tradieProfile.City != tradie.City)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.City, tradieProfile.City));
            }
            if (!string.IsNullOrEmpty(tradieProfile.State) && tradieProfile.State != tradie.State)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.State, tradieProfile.State));
            }
            if (!string.IsNullOrEmpty(tradieProfile.PostalCode) && tradieProfile.PostalCode != tradie.PostalCode)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.PostalCode, tradieProfile.PostalCode));
            }
            if (!string.IsNullOrEmpty(tradieProfile.ContactNumber) && tradieProfile.ContactNumber != tradieProfile.ContactNumber)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ContactNumber, tradieProfile.ContactNumber));
            }

            // Update Profile Picture if provided
            if (!string.IsNullOrEmpty(tradieProfile.ProfilePicture) && tradieProfile.ProfilePicture != tradie.ProfilePicture)
            {
                updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProfilePicture, tradieProfile.ProfilePicture));
            }

            // Handle the update of Certifications
            if (tradieProfile.CertificationFilesUploaded != null)
            {
                // Ensure tradie.Services is initialized (default to empty list if null)
                var currentCertifications= tradie.CertificationFilesUploaded ?? new List<string>();

                // Compare lists considering possible null values
                if (!tradieProfile.CertificationFilesUploaded.SequenceEqual(currentCertifications))
                {
                    updateDefinitions.Add(Builders<User>.Update.Set(u => u.CertificationFilesUploaded, tradieProfile.CertificationFilesUploaded));
                }
            }


            // Handle the update of services
            if (tradieProfile.Services != null)
            {
                // Ensure tradie.Services is initialized (default to empty list if null)
                var currentServices = tradie.Services ?? new List<string>();

                // Compare lists considering possible null values
                if (!tradieProfile.Services.SequenceEqual(currentServices))
                {
                    updateDefinitions.Add(Builders<User>.Update.Set(u => u.Services, tradieProfile.Services));
                }
            }

            if (updateDefinitions.Count == 0)
            {
                return BadRequest(new { message = "No valid fields to update" });
            }

            try
            {
                var updateDefinition = Builders<User>.Update.Combine(updateDefinitions);
                var filter = Builders<User>.Filter.Eq(u => u._id, tradieProfile.ID);

                _context.User.UpdateOne(filter, updateDefinition);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile", error = ex.Message });
            }

            return Ok(new { message = "Tradie profile updated successfully" });
        }

        [HttpPut("update-tradie-profile-picture")]
        public IActionResult UpdateTradieProfilePicture([FromBody] UpdateTradieProfilePicture tradieProfile)
        {
            var tradie = _context.User.Find(user => user._id == tradieProfile.ID && user.Role == "Tradie").FirstOrDefault();
            if (tradie == null)
            {
                return NotFound(new { message = "Tradie not found" });
            }

            var updateDefinitions = new List<UpdateDefinition<User>>();

            if (tradieProfile.ProfilePicture != null && tradieProfile.ProfilePicture.Length > 0)
            {              
                if (!string.IsNullOrEmpty(tradie.ProfilePicture))
                {
                    updateDefinitions.Add(Builders<User>.Update.Set(u => u.ProfilePicture, tradie.ProfilePicture));
                }
            }

            if (updateDefinitions.Count == 0)
            {
                return BadRequest(new { message = "No valid fields to update" });
            }

            try
            {
                var updateDefinition = Builders<User>.Update.Combine(updateDefinitions);
                var filter = Builders<User>.Filter.Eq(u => u._id, tradieProfile.ID);

                _context.User.UpdateOne(filter, updateDefinition);
            }
            catch (Exception ex)
            {
                // Log the exception and return a generic error message
                return StatusCode(500, new { message = "An error occurred while updating the profile picture", error = ex.Message });
            }

            return Ok(new { message = "Tradie profile picture updated successfully" });
        }

        [HttpPost("add-tradie-job-ad")]
        public IActionResult AddTradieJobAd([FromBody] Services jobPost)
        {
            try
            {
                var tradie = _context.User.Find(user => user._id == jobPost.UserID && user.Role == "Tradie").FirstOrDefault();
                if (tradie == null)
                {
                    return NotFound(new { message = "Tradie not found" });
                }

                var existingService = _context.Services.Find(service => service.JobAdTitle == jobPost.JobAdTitle).FirstOrDefault();
                if (!(existingService==null))
                {
                    return NotFound(new { message = "Job Post Ad already exists" });
                }

                    _context.Services.InsertOne(jobPost);
                    return Ok(new { message = "Tradie job post added successfully" });
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding job post", error = ex.Message });
            }
        }       

        [HttpPost("update-active-jobs-count")]
        public IActionResult UpdateActiveJobsCount([FromBody] UpdateActiveJobs activeJob)
        {
            var tradie = _context.User.Find(user => user._id == activeJob.UserID && user.Role == "Tradie").FirstOrDefault();
            if (tradie == null)
            {
                return NotFound(new { message = "Tradie not found" });
            }

            // Update the status
            var updateDefinition = Builders<User>.Update.Set(u => u.ActiveJobs, activeJob.ActiveJobCount);
            _context.User.UpdateOne(user => user._id == activeJob.UserID, updateDefinition);

            return Ok(new { message = "Active jobs updated" });
        }

        [HttpGet("publishedAds")]
        public IActionResult GetPublishedAds([FromBody] GetPublishedAd getPublishedAd)
        {
            var publishedJobPost = _context.Services.Find(service => service.UserID == getPublishedAd.UserID && service.IsActive == true).ToList();

            return Ok(publishedJobPost);
        }

        [HttpGet("unpublishedAds")]
        public IActionResult GetUnpublishedAds([FromBody] GetPublishedAd getunPublishedAd)
        {
            var publishedJobPost = _context.Services.Find(service => service.UserID == getunPublishedAd.UserID && service.IsActive == false).ToList();

            return Ok(publishedJobPost);
        }

        [HttpPut("job-ads/update-isActive")]
        public IActionResult UpdateIsActiveJobAds([FromBody] UpdateJobAdsIsActive updateJobAdsIsActive)
        {
            var publishedJobPost = _context.Services.Find(service => service._id == updateJobAdsIsActive.JobID).FirstOrDefault();
            if (publishedJobPost == null)
            {
                return NotFound(new { message = "Job Ad not found" });
            }

            var updateDefinition = Builders<Services>.Update.Set(u => u.IsActive, updateJobAdsIsActive.IsActive);
            _context.Services.UpdateOne(service => service._id == updateJobAdsIsActive.JobID, updateDefinition);

            return Ok(new { message = "Job Ad successfully updated" });
        }

        [HttpGet("job-ad-getDetails-byServiceID")]
        public IActionResult GetJobDetailsByServiceId([FromBody] GetJobDetailsByServiceId getJobDetails)
        {
            var service = _context.Services.Find(service => service._id == getJobDetails.ServiceID).FirstOrDefault();
            if (service == null)
            {
                return NotFound(new { message = "Job post not found" });
            }
            return Ok(service);
        }

        [HttpPut("update-job-ad-Details")]
        public IActionResult UpdateJobAdDetails([FromBody] Services updatedJobPost)
        {
            var service = _context.Services.Find(service => service._id == updatedJobPost._id).FirstOrDefault();
            if (service == null)
            {
                return NotFound(new { message = "Job post ad not found" });
            }

            var updateDefinitions = new List<UpdateDefinition<Services>>();

            // Update only the non-null values from the updatedJobPost

            if (!string.IsNullOrEmpty(updatedJobPost.BusinessPostcode))
            {
                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.BusinessPostcode, updatedJobPost.BusinessPostcode));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.JobCategory))
            {
                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.JobCategory, updatedJobPost.JobCategory));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.JobAdTitle))
            {

                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.JobAdTitle, updatedJobPost.JobAdTitle));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.DescriptionOfService))
            {

                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.DescriptionOfService, updatedJobPost.DescriptionOfService));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.PricingOption))
            {

                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.PricingOption, updatedJobPost.PricingOption));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.PricingStartsAt))
            {

                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.PricingStartsAt, updatedJobPost.PricingStartsAt));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.Currency))
            {

                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.Currency, updatedJobPost.Currency));
            }

            if (!string.IsNullOrEmpty(updatedJobPost.ThumbnailImage))
            {

                updateDefinitions.Add(Builders<Services>.Update.Set(u => u.ThumbnailImage, updatedJobPost.ThumbnailImage));
            }

            if (updatedJobPost.ProjectGallery != null)
            {
                var currentProjectGallery = service.ProjectGallery ?? new List<string>();

                if (!updatedJobPost.ProjectGallery.SequenceEqual(currentProjectGallery))
                {
                    updateDefinitions.Add(Builders<Services>.Update.Set(u => u.ProjectGallery, updatedJobPost.ProjectGallery));
                }
            }


            if (updateDefinitions.Count == 0)
            {
                return BadRequest(new { message = "No valid fields to update" });
            }

            try
            {
                var updateDefinition = Builders<Services>.Update.Combine(updateDefinitions);
                var filter = Builders<Services>.Filter.Eq(u => u._id, updatedJobPost._id);

                _context.Services.UpdateOne(filter, updateDefinition);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the profile", error = ex.Message });
            }

            return Ok(new { message = "Job post ad updated successfully" });
        }

    }
}
