using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class CommentReviewController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public CommentReviewController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        [Route("CommentReviews")]
        public async Task<IEnumerable<CommentReview>> CommentReview()
        {
            var comment = await _dbContext.CommentReview
                .Include(c => c.LikeCommentReview)
                .ToListAsync();

            var filteredComment = comment
                .Select(c =>
                {
                    if (c.LikeCommentReview != null)
                    {
                        c.ScoreLike = c.LikeCommentReview.Count(l => l.Like == 1);
                    }
                    c.LikeCommentReview = null;
                    return c;
                })
                .ToList();

            return filteredComment;
        }

        [HttpGet]
        [Route("CommentReview/{reviewId}")]
        public async Task<ActionResult<List<CommentReview>>> CommentReviewById(int reviewId)
        {   

            var dataReview = await _dbContext.Review
                .FirstOrDefaultAsync(p => p.ReviewId == reviewId);

            if (dataReview == null)
            {
                return NotFound("ReviewId not found");
            }

            var comments = await _dbContext.CommentReview
                .Where(c => c.ReviewId == reviewId)
                .ToListAsync();

            return comments;
        }


        [HttpPost]
        [Route("CommentReview")]
        public async Task<ActionResult<CommentReview>> Add([FromForm] CommentReviewCreate objCR)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var datar = await _dbContext.Review
          .FirstOrDefaultAsync(p => p.ReviewId == objCR.ReviewId);

            if (datar == null)
            {
                return NotFound("ReviewId not found");
            }

            var dataUser = await _dbContext.User
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (dataUser == null)
            {
                return NotFound("UserId not found");
            }


            var cpostEntity = new CommentReview
                {
                    ReviewId = objCR.ReviewId,
                    Text = objCR.Text,
                    UserId = userId,
                };

                _dbContext.CommentReview.Add(cpostEntity);
                await _dbContext.SaveChangesAsync();

                return cpostEntity;
        }

        [HttpDelete]
        [Route("CommentReview")]
        public async Task<ActionResult<CommentReview>> Delete(int id)
        {
            var CommentToDelete = await _dbContext.CommentReview.FindAsync(id);

            if (CommentToDelete == null)
            {
                return NotFound();
            }

            _dbContext.CommentReview.Remove(CommentToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(CommentToDelete);
        }

        [HttpPatch]
        [Route("CommentReview")]
        public async Task<IActionResult> Edit(int id, [FromForm] CommentReviewUpdate editComementReview)
        {
            var commentToUpdate = await _dbContext.CommentReview.FindAsync(id);

            if (commentToUpdate == null)
            {
                return NotFound();
            }

            commentToUpdate.Text = editComementReview.Text;      

            await _dbContext.SaveChangesAsync();

            return Ok(commentToUpdate);
        }


    }
}
