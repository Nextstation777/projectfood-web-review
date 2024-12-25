using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public UserController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("Users")]
        public async Task<IEnumerable<User>> Users()
        {
            var users = await _dbContext.User
                .Include(u => u.Review)
                .Include(u => u.Shop)
                .ToListAsync();

            foreach (var user in users)
            {
               
                user.ReviewCount = user.Review?.Count ?? 0;

                user.ProfilePic = AddServerUrlToProfilePic(user.ProfilePic);
            }

            return users;
        }

        private string AddServerUrlToProfilePic(string filename)
        {
            string serverUrl = "https://localhost:7199/ProfilePic/";
            return serverUrl + filename;
        }

        [HttpGet]
        [Route("User/{userName}")]
        public async Task<ActionResult<User>> GetUserByName(string userName)
        {
            var user = await _dbContext.User
                .Include(u => u.Review)
                .Include(u => u.Shop)
                .FirstOrDefaultAsync(u => u.UserName == userName); ;

            if (user == null)
            {
                return NotFound(); 
            }

            user.ReviewCount = user.Review?.Count ?? 0;

          
            user.ProfilePic = AddServerUrlToProfilePic(user.ProfilePic);

            return user;
        }


        [HttpPost]
        [Route("User")]
        public async Task<ActionResult<User>> Add([FromForm] Usercreate objUser)
        {
          
                var User = await _dbContext.User.FirstOrDefaultAsync(u =>
                u.UserName == objUser.UserName ||
                u.Email == objUser.Email);

                if (User != null)
                {
                    return BadRequest("User with the same username, email already exists.");
                }
                var userentity = new User
            {
                UserName = objUser.UserName,
                Email = objUser.Email,
                Password = objUser.Password,
                Name = objUser.Name,
                State = "1",
                };

            
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (objUser.ProfilePic != null)
            {
                var fileExtension = Path.GetExtension(objUser.ProfilePic.FileName);

                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                {
                    return BadRequest("Invalid file format. Please upload a valid image file.");
                }
                    
                        var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ProfilePic");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var randomFileName = Path.GetRandomFileName();
                        var fileNameWithExtension = Path.ChangeExtension(randomFileName, fileExtension);
                        var filePath = string.Concat(path,"/", fileNameWithExtension);

                    
                        using (FileStream fs = System.IO.File.Create(filePath))
                        {
                            objUser.ProfilePic.CopyTo(fs);
                            fs.Flush();
                        }

                    userentity.ProfilePic = fileNameWithExtension;
               

            }
             var thaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
             userentity.TimeRegister = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, thaiTimeZone).ToString("yyyy-MM-dd HH:mm:ss");


                _dbContext.User.Add(userentity);
            await _dbContext.SaveChangesAsync();
            return userentity;

        }

        [HttpDelete]
        [Route("User")]
        public async Task<ActionResult<User>> Delete([FromForm] string password)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var userToDelete = await _dbContext.User.FindAsync(userId);

            if (userToDelete == null)
            {
                return NotFound();
            }

            if (userToDelete.Password != password)
            {
                return BadRequest("Incorrect password.");
            }

            // Remove associated shops
            var shopsToDelete = await _dbContext.Shop.Where(s => s.UserId == userId).ToListAsync();
            _dbContext.Shop.RemoveRange(shopsToDelete);

            // Remove associated reviews
            var reviewsToDelete = await _dbContext.Review.Where(r => r.UserId == userId).ToListAsync();
            _dbContext.Review.RemoveRange(reviewsToDelete);

            // Remove user
            _dbContext.User.Remove(userToDelete);

            await _dbContext.SaveChangesAsync();

            return Ok(userToDelete);
        }

        [HttpPatch]
        [Route("User")]
        public async Task<IActionResult> Update([FromForm] UserUpdate updatedUserData)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var userToUpdate = await _dbContext.User.FindAsync(userId);

            if (userToUpdate == null)
            {
                return NotFound();
            }

            if (VerifyPassword(updatedUserData.Password, userToUpdate.Password))
            {
                userToUpdate.Name = updatedUserData.Name;

                if (updatedUserData.ProfilePic != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await updatedUserData.ProfilePic.CopyToAsync(memoryStream);


                        if (!string.IsNullOrEmpty(userToUpdate.ProfilePic))
                        {
                            var oldFilePath = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ProfilePic", userToUpdate.ProfilePic);
                            System.IO.File.Delete(oldFilePath);
                        }
                        var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ProfilePic");
                        var randomFileName = Path.GetRandomFileName();
                        var fileNameWithExtension = Path.ChangeExtension(randomFileName, ".jpg");
                        userToUpdate.ProfilePic = fileNameWithExtension;

                        var filePath = string.Concat(path, "/", fileNameWithExtension);
                        await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
                    }
                }

                await _dbContext.SaveChangesAsync();

                return Ok(userToUpdate);
            }
            else
            {
                return BadRequest("Incorrect password");
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedPassword)
        {
            return enteredPassword == storedPassword;
        }

    }
}
