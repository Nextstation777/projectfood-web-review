using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class CommentPostController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public CommentPostController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        [Route("CommentPosts")]
        public async Task<IEnumerable<CommentPost>> GetCommentPosts()
        {
            var commentPosts = await _dbContext.CommentPost
                .Include(c => c.LikeCommentPost)
                .ToListAsync();

            var filteredCommentPosts = commentPosts
                .Select(c =>
                {
                    if (c.LikeCommentPost != null)
                    {
                        c.ScoreLike = c.LikeCommentPost.Count(l => l.Like == 1);
                    }
                    c.LikeCommentPost = null;
                    return c;
                })
                .ToList();

            return filteredCommentPosts;
        }



        [HttpGet]
        [Route("CommentPost/{postId}")]
        public async Task<ActionResult<List<CommentPost>>> GetCommentById(int postId)
        {
            var dataPost = await _dbContext.Post
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (dataPost == null)
            {
                return NotFound("PostId not found");
            }

            var comments = await _dbContext.CommentPost
                .Where(c => c.PostId == postId)
                .ToListAsync();

            return comments;
        }


        [HttpPost]
        [Route("CommentPost")]
        public async Task<ActionResult<CommentPost>> Add([FromForm] CommentPostCreate objCPost)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var dataPost = await _dbContext.Post
          .FirstOrDefaultAsync(p => p.PostId == objCPost.PostId);

            if (dataPost == null)
            {
                return NotFound("PostId not found");
            }

            var dataUser = await _dbContext.User
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (dataUser == null)
            {
                return NotFound("UserId not found");
            }


            var cpostEntity = new CommentPost
                {
                    PostId = objCPost.PostId,
                    Text = objCPost.Text,
                    UserId = userId,
                };

                _dbContext.CommentPost.Add(cpostEntity);
                await _dbContext.SaveChangesAsync();

                return cpostEntity;
        }

        [HttpDelete]
        [Route("CommentPost")]
        public async Task<ActionResult<CommentPost>> Delete(int id)
        {
            var CommentToDelete = await _dbContext.CommentPost.FindAsync(id);

            if (CommentToDelete == null)
            {
                return NotFound();
            }

            _dbContext.CommentPost.Remove(CommentToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(CommentToDelete);
        }

        [HttpPatch]
        [Route("CommentPost")]
        public async Task<IActionResult> Edit(int id, [FromForm] CommentPostUpdate editComementPost)
        {
            var commentToUpdate = await _dbContext.CommentPost.FindAsync(id);

            if (commentToUpdate == null)
            {
                return NotFound();
            }

            commentToUpdate.Text = editComementPost.Text;      

            await _dbContext.SaveChangesAsync();

            return Ok(commentToUpdate);
        }


    }
}
