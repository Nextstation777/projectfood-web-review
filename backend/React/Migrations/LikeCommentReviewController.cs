using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class LikeCommentReviewController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public LikeCommentReviewController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("LikeCommentReviews")]
        public async Task<IEnumerable<LikeCommentReview>> LikeCommentReview()
        {
            var LikeCommentPost = await _dbContext.LikeCommentReview.ToListAsync();
            return LikeCommentPost;
        }

        [HttpGet]
        [Route("LikeCommentReview/{commentId}")]
        public async Task<ActionResult<List<LikeCommentReview>>> LikeCommentReviewById(int commentId)
        {
            var dataComment = await _dbContext.LikeCommentReview
                .FirstOrDefaultAsync(c => c.CommentReviewId == commentId);

            if (dataComment == null)
            {
                return NotFound("CommentId not found");
            }

            var likes = await _dbContext.LikeCommentReview
                .Where(l => l.CommentReviewId == commentId)
                .ToListAsync();

            return likes;
        }



        [HttpPost]
        [Route("LikeCommentReview")]
        public async Task<ActionResult<LikeCommentReview>> Add([FromForm] LikeCommentReviewCreate objC)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            try
            {
                var existingLike = await _dbContext.LikeCommentReview
                    .FirstOrDefaultAsync(l =>
                        l.UserId == userId &&
                        l.CommentReviewId == objC.CommentReviewId);

                if (existingLike != null)
                {
                    _dbContext.LikeCommentReview.Remove(existingLike);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Unlike");
                }

                var commentReview = await _dbContext.CommentReview
                    .FirstOrDefaultAsync(c => c.CommentReviewId == objC.CommentReviewId);

                if (commentReview == null)
                {
                    return NotFound("Comment review not found");
                }

                var newLike = new LikeCommentReview
                {
                    CommentReviewId = objC.CommentReviewId,
                    UserId = userId,
                    Like = 1,
                };

                _dbContext.LikeCommentReview.Add(newLike);
                await _dbContext.SaveChangesAsync();

                return newLike;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("LikeCommentReview")]
        public async Task<ActionResult<LikeCommentReview>> Delete(int id)
        {
            var CommentToDelete = await _dbContext.LikeCommentReview.FindAsync(id);

            if (CommentToDelete == null)
            {
                return NotFound();
            }

            _dbContext.LikeCommentReview.Remove(CommentToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(CommentToDelete);
        }



    }
}
