using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class LikeCommentPostController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public LikeCommentPostController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("LikeCommentPosts")]
        public async Task<IEnumerable<LikeCommentPost>> LikeCommentPost()
        {
            var LikeCommentPost = await _dbContext.LikeCommentPost.ToListAsync();
            return LikeCommentPost;
        }

        [HttpGet]
        [Route("LikeCommentPost/{commentId}")]
        public async Task<ActionResult<List<LikeCommentPost>>> GetLikeCommentPostById(int commentId)
        {
            var dataComment = await _dbContext.CommentPost
                .FirstOrDefaultAsync(c => c.CommentPostId == commentId);

            if (dataComment == null)
            {
                return NotFound("CommentId not found");
            }

            var likes = await _dbContext.LikeCommentPost
                .Where(l => l.CommentPostId == commentId)
                .ToListAsync();

            return likes;
        }



        [HttpPost]
        [Route("LikeCommentPost")]
        public async Task<ActionResult<LikeCommentPost>> Add([FromForm] LikeCommentPostCreate objC)
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            try
            {
                var existingLike = await _dbContext.LikeCommentPost
                    .FirstOrDefaultAsync(l =>
                        l.UserId == userId &&
                        l.CommentPostId == objC.CommentPostId);

                if (existingLike != null)
                {
                    _dbContext.LikeCommentPost.Remove(existingLike);
                    await _dbContext.SaveChangesAsync();
                    return Ok("Unlike");
                }

                var dataPost = await _dbContext.CommentPost
                    .FirstOrDefaultAsync(c => c.CommentPostId == objC.CommentPostId);

                if (dataPost == null)
                {
                    return NotFound("CommentPostId not found");
                }

                var dataUser = await _dbContext.User
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (dataUser == null)
                {
                    return NotFound("UserId not found");
                }

                var newLike = new LikeCommentPost
                {
                    CommentPostId = objC.CommentPostId,
                    UserId = userId,
                    Like = 1,
                };

                _dbContext.LikeCommentPost.Add(newLike);
                await _dbContext.SaveChangesAsync();

                return newLike;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("LikeCommentPost")]
        public async Task<ActionResult<LikeCommentPost>> Delete(int id)
        {
            var CommentToDelete = await _dbContext.LikeCommentPost.FindAsync(id);

            if (CommentToDelete == null)
            {
                return NotFound();
            }

            _dbContext.LikeCommentPost.Remove(CommentToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(CommentToDelete);
        }



    }
}
