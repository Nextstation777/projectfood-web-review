using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class RatingReviewController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public RatingReviewController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("RatingReviews")]
        public async Task<IEnumerable<RatingReview>> RatingReview()
        {
            return await _dbContext.RatingReview.ToListAsync();
        }

        [HttpPost]
        [Route("RatingReview")]
        public async Task<ActionResult<RatingReview>> Add([FromForm] RatingReviewCreate objR)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            if (objR.ReviewScore == 0)
            {
                var existingRating = await _dbContext.RatingReview
                    .FirstOrDefaultAsync(r => r.ReviewId == objR.ReviewId && r.UserId == userId);

                if (existingRating != null)
                {
                    _dbContext.RatingReview.Remove(existingRating);
                    await _dbContext.SaveChangesAsync();
                }

                return NoContent();
            }
            else
            {
                var existingRating = await _dbContext.RatingReview
                    .FirstOrDefaultAsync(r => r.ReviewId == objR.ReviewId && r.UserId == userId);

                if (existingRating != null)
                {
                    existingRating.ReviewScore = objR.ReviewScore;
                    await _dbContext.SaveChangesAsync();
                    return existingRating;
                }

                var reviewentity = new RatingReview
                {
                    ReviewId = objR.ReviewId,
                    UserId = userId,
                    ReviewScore = objR.ReviewScore,
                };

                _dbContext.RatingReview.Add(reviewentity);
                await _dbContext.SaveChangesAsync();
                return reviewentity;
            }
        }


    }
}
