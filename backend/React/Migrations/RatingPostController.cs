using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class RatingPostController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public RatingPostController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("RatingPosts")]
        public async Task<IEnumerable<RatingPost>> RatingPost()
        {
            return await _dbContext.RatingPost.ToListAsync();
        }

        [HttpPost]
        [Route("RatingPost")]
        public async Task<ActionResult<RatingPost>> Add([FromForm] RatingPostCreate objPost)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            if (objPost.PostScore == 0)
            {
                var existingRating = await _dbContext.RatingPost
                    .FirstOrDefaultAsync(r => r.PostId == objPost.PostId && r.UserId == userId);

                if (existingRating != null)
                {
                    _dbContext.RatingPost.Remove(existingRating);
                    await _dbContext.SaveChangesAsync();
                }

                return NoContent(); 
            }
            else
            {
                var existingRating = await _dbContext.RatingPost
                    .FirstOrDefaultAsync(r => r.PostId == objPost.PostId && r.UserId == userId);

                if (existingRating != null)
                {
                    existingRating.PostScore = objPost.PostScore;
                    await _dbContext.SaveChangesAsync();
                    return existingRating;
                }

                var postentity = new RatingPost
                {
                    PostId = objPost.PostId,
                    UserId = userId,
                    PostScore = objPost.PostScore,
                };

                _dbContext.RatingPost.Add(postentity);
                await _dbContext.SaveChangesAsync();
                return postentity;
            }
        }
       


    }
}
