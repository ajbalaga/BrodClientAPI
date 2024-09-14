using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BrodClientAPI.Data;
using BrodClientAPI.Models;
using MongoDB.Driver;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using SendGrid;
using SendGrid.Helpers.Mail;


namespace BrodClientAPI.Controller
{
        [ApiController]
        [Route("api/[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly ApiDbContext _context;
            private readonly IConfiguration _configuration;

            public AuthController(ApiDbContext context, IConfiguration configuration)
                {
                _context = context;
                _configuration = configuration;
            }

            [HttpPost("login")]
            public IActionResult Login([FromBody] LoginInput login)
            {
                try
                {
                    var allUsers = _context.User.Find(_ => true).ToList();
                    var user = _context.User.Find(u => u.Email == login.Email && u.Password == login.Password).FirstOrDefault();

                    if (user == null)
                        return Unauthorized();

                    var token = GenerateJwtToken(user);
                    return Ok(new { token, userId = user._id });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "An error occurred while adding job post", error = ex.Message });
                }
            }
            private string GenerateJwtToken(User user)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var secretKey = _configuration["JwtSettings:SecretKey"];
                    var issuer = _configuration["JwtSettings:Issuer"];
                    var audience = _configuration["JwtSettings:Audience"];

                    var key = Encoding.ASCII.GetBytes(secretKey);

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new[]
                        {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                        Expires = DateTime.UtcNow.AddHours(1),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = issuer,
                        Audience = audience
                    };

                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    return tokenHandler.WriteToken(token);
                }

            [HttpPost("signup")]
            public async Task<IActionResult> Signup([FromBody] User userSignupDto)
            {
            try
            {
                // Check if the user already exists
                var existingUser = _context.User.Find(u => u.Email == userSignupDto.Email).FirstOrDefault();
                if (existingUser != null)
                    return BadRequest("User already exists");

                // Add the new user to the database
                _context.User.InsertOne(userSignupDto);

                // Return a success response
                return Ok(new { message = "Signup successful" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding job post", error = ex.Message });
            }
        }

            private string HashPassword(string password)
            {
                // Add your password hashing logic here, e.g., using BCrypt or another hashing algorithm.
                return password; // Replace this with the actual hashed password.
            }

            //for OTP login and signup
            private void SendSmsOtp(string phoneNumber, string otp)
            {
                var acctsid = _configuration["Twilio:ACCOUNT_SID"];
                var token = _configuration["Twilio:AUTH_TOKEN"];

                TwilioClient.Init(acctsid, token);

                var message = MessageResource.Create(
                    body: $"Your OTP code is {otp}",
                    from: new PhoneNumber("Your Twilio Number"),
                    to: new PhoneNumber(phoneNumber)
                );
            }

            private async Task SendEmailOtp(string emailAddress, string otp)
            {
                var apikey = _configuration["SendGrid:API_KEY"];
                var email = _configuration["SendGrid:Email"];
                var appName = _configuration["SendGrid:AppName"];

                var client = new SendGridClient(apikey);
                var from = new EmailAddress(email, appName);
                var subject = "Your OTP Code";
                var to = new EmailAddress(emailAddress);
                var plainTextContent = $"Your OTP code is {otp}";
                var htmlContent = $"<strong>Your OTP code is {otp}</strong>";
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
                var response = await client.SendEmailAsync(msg);
            }

        }
    
}
