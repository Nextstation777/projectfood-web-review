using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // Add this using directive
using Microsoft.IdentityModel.Tokens;
using React.Models;
using React.share;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace React.Migrations
{
    [ApiController]
    [Route("")]
    public class LoginController : ControllerBase
    {
        private readonly SystemDbContext _dbContext;
        private readonly IConfiguration _configuration; 

        public LoginController(SystemDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration; 
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<ActionResult<LoginResponse>> Login([FromForm] LoginRequest loginRequest)
        {
            try
            {
                var user = await _dbContext.User
                    .Where(u => (u.Email == loginRequest.Email || u.UserName == loginRequest.Email) && u.Password == loginRequest.Password)
                    .FirstOrDefaultAsync();

                var adminUser = await _dbContext.Admin
                    .Where(u => u.Email == loginRequest.Email && u.Password == loginRequest.Password)
                    .FirstOrDefaultAsync();

                if (user == null && adminUser == null)
                {
                    return NotFound("Invalid Email or UserName or password");
                }

                var response = new LoginResponse();

                if (user != null)
                {
                    response.Name = user.Name;
                    response.State = user.State;
                    response.UserId = user.UserId;
                    response.UserName = user.UserName;
                    response.Role = "User";

                    // Generate and attach JWT token for User
                    var tokenUser = GenerateJwtToken(user);
                    response.Token = tokenUser;
                }
                else if (adminUser != null)
                {
                    response.Name = adminUser.Name;
                    response.State = adminUser.State;
                    response.Role = "Admin";

                    // Generate and attach JWT token for Admin
                    var tokenAdmin = GenerateJwtToken(adminUser);
                    response.Token = tokenAdmin;
                }

                return response;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        public class LoginResponse
        {
            public int UserId { get; set; }
            public string UserName { get; set; }
            public string Name { get; set; }
            public string State { get; set; }
            public string Role { get; set; }
            public string Token { get; set; }


        }

        private string GenerateJwtToken(object user)
        {
            var issuer = _configuration["Jwt:Issuer"];
            var key = _configuration["Jwt:Key"];

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = (user is User) ?
                    new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.NameIdentifier, ((User)user).UserName),
                new Claim(ClaimTypes.Email, ((User)user).Email),
                new Claim("UserId", ((User)user).UserId.ToString()),
                new Claim("Name", ((User)user).Name.ToString()),
                    }) :
                    new ClaimsIdentity(new Claim[]
                    {
                new Claim(ClaimTypes.NameIdentifier, ((Admin)user).Email),
                new Claim(ClaimTypes.Role, "Admin"),
                    }),
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = issuer,
                Audience = issuer, 
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
