using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class PostController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public PostController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("Posts")]
        public async Task<IEnumerable<PostWithShop>> Post()
        {
            var posts = await _dbContext.Post
                .Include(p => p.Shop)
                .Include(p => p.ImagePost)
                .Include(p => p.RatingPost)
                .Select(p => new
                {
                    p.PostId,
                    p.Topic,
                    p.Text,
                    p.CreateTime,
                    p.CoverPhoto,
                    p.ShopId,
                    p.Shop,
                    ShopName = p.Shop.ShopName,
                    ImagePost = p.ImagePost,
                    PostScore = CalculateAveragePostScore(p.RatingPost),
                })
                .ToListAsync();

            var result = posts.Select(post =>
            {
                var imagePosts = post.ImagePost?.Select(ImagePost => $"https://localhost:7199/ImagePost/{ImagePost?.Images}").ToList();
                return new PostWithShop
                {
                    PostId = post.PostId,
                    Topic = post.Topic,
                    Text = post.Text,
                    CreateTime = post.CreateTime, // Format the DateTime
                    ShopName = post.ShopName,
                    ShopId = post.ShopId,
                    Shop = post.Shop,
                    PostScore = post.PostScore,
                    CoverPhotoUrl = GetCoverPhotoUrl(post.CoverPhoto),
                    ImagePost = imagePosts,
                };
            });

            return result;
        }

        private string GetCoverPhotoUrl(string? coverPhoto)
        {
            return string.IsNullOrEmpty(coverPhoto) ? string.Empty : $"https://localhost:7199/CoverPhotoPost/{coverPhoto}";
        }

        private static int CalculateAveragePostScore(List<RatingPost>? ratingPosts)
        {
            if (ratingPosts == null || ratingPosts.Count == 0)
            {
                return 0; // Return 0 if there are no ratings
            }

            int totalScore = ratingPosts.Sum(rp => rp.PostScore);
            int averageScore = totalScore / ratingPosts.Count;

            return averageScore;
        }

        public class PostWithShop
        {
            public int PostId { get; set; }
            public string? ShopName { get; set; }
            public int ShopId { get; set; }
            public string? Text { get; set; }
            public string? Topic { get; set; }
            public string? CreateTime { get; set; }
            public string? CoverPhotoUrl { get; set; }
            public int PostScore { get; set; }

            public Shop Shop { get; set; }
            public List<string?> ImagePost { get; set; }
        }

        [HttpGet]
        [Route("Post/{postId}")]
        public async Task<IActionResult> PostById(int postId)
        {
            var post = await _dbContext.Post
                .Include(p => p.Shop)
                .Include(p => p.ImagePost)
                .Include(p => p.RatingPost)
                .Where(p => p.PostId == postId)
                .Select(p => new
                {
                    p.PostId,
                    p.Topic,
                    p.Text,
                    p.CreateTime,
                    p.CoverPhoto,
                    p.ShopId,
                    p.Shop,
                    ShopName = p.Shop.ShopName,
                    ImagePost = p.ImagePost,
                    PostScore = CalculateAveragePostScore(p.RatingPost),
                })
                .FirstOrDefaultAsync();

            if (post == null)
            {
                return NotFound();
            }

            var imagePosts = post.ImagePost?.Select(ImagePost => $"https://localhost:7199/ImagePost/{ImagePost?.Images}").ToList();

            var result = new PostWithShop
            {
                PostId = post.PostId,
                Topic = post.Topic,
                Text = post.Text,
                CreateTime = post.CreateTime, // Format the DateTime
                ShopName = post.ShopName,
                ShopId = post.ShopId,
                Shop = post.Shop,
                PostScore = post.PostScore,
                CoverPhotoUrl = GetCoverPhotoUrl(post.CoverPhoto),
                ImagePost = imagePosts,
            };

            return Ok(result);
        }


        [HttpPost]
        [Route("Post")]
        public async Task<ActionResult<Post>> Add([FromForm] PostCreate objPost)
        {
            try
            {
                // Get UserId from token
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Invalid or missing user information in the token.");
                }

                // Find ShopId from UserId
                var shopId = await _dbContext.Shop
                    .Where(u => u.UserId == userId)
                    .Select(u => u.ShopId)
                    .FirstOrDefaultAsync();

                if (shopId == null)
                {
                    return BadRequest("User does not have a shop.");
                }

                // Check if the user already has a post
                var existingPost = await _dbContext.Post.FirstOrDefaultAsync(p => p.ShopId == shopId);
                if (existingPost != null)
                {
                    return BadRequest("The user already has a post.");
                }

                var postEntity = new Post
                {
                    Topic = objPost.Topic,
                    Text = objPost.Text,
                    ShopId = shopId,
                };

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (objPost.CoverPhoto != null)
                {
                    var fileExtension = Path.GetExtension(objPost.CoverPhoto.FileName);

                    if (!allowedExtensions.Contains(fileExtension.ToLower()))
                    {
                        return BadRequest("Invalid file format. Please upload a valid image file.");
                    }

                    var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\CoverPhotoPost");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var randomFileName = Path.GetRandomFileName();
                    var fileNameWithExtension = Path.ChangeExtension(randomFileName, fileExtension);
                    var filePath = Path.Combine(path, fileNameWithExtension);

                    using (FileStream fs = System.IO.File.Create(filePath))
                    {
                        objPost.CoverPhoto.CopyTo(fs);
                        fs.Flush();
                    }

                    var thaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    postEntity.CreateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, thaiTimeZone).ToString("yyyy-MM-dd HH:mm:ss");

                    postEntity.CoverPhoto = fileNameWithExtension;
                }

                _dbContext.Post.Add(postEntity);
                await _dbContext.SaveChangesAsync();

                return postEntity;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }


        [HttpDelete]
        [Route("Post")]
        public async Task<ActionResult<Post>> Delete()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            // Find ShopId using UserId
            var shop = await _dbContext.Shop
                .FirstOrDefaultAsync(u => u.UserId == userId);

            if (shop == null)
            {
                return NotFound("Shop not found for the user.");
            }

            // Find the post to delete
            var postToDelete = await _dbContext.Post
                .FirstOrDefaultAsync(p => p.ShopId == shop.ShopId);

            if (postToDelete == null)
            {
                return NotFound("Post not found for the user.");
            }

            // Check if the user owns the post before deleting
            if (postToDelete.ShopId != shop.ShopId)
            {
                return Forbid("You are not authorized to delete this post.");
            }

            _dbContext.Post.Remove(postToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(postToDelete);
        }




        [HttpPatch]
        [Route("Post")]
        public async Task<IActionResult> Edit([FromForm] PostUpdate objPost)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            // Find ShopId using UserId
            var shop = await _dbContext.Shop.FirstOrDefaultAsync(u => u.UserId == userId);

            if (shop == null)
            {
                return NotFound("Shop not found for the user.");
            }

            // Find the post to update
            var postToUpdate = await _dbContext.Post.FindAsync(userId);

            if (postToUpdate == null)
            {
                return NotFound();
            }

            // Check if the post belongs to the user's shop
            if (postToUpdate.ShopId != shop.ShopId)
            {
                return Forbid("You are not authorized to edit this post.");
            }

            postToUpdate.Topic = objPost.Topic;
            postToUpdate.Text = objPost.Text;

            if (objPost.CoverPhoto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await objPost.CoverPhoto.CopyToAsync(memoryStream);

                    // Delete old cover photo
                    if (!string.IsNullOrEmpty(postToUpdate.CoverPhoto))
                    {
                        var oldFilePath = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\CoverPhotoPost", postToUpdate.CoverPhoto);
                        System.IO.File.Delete(oldFilePath);
                    }

                    // Save new cover photo
                    var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\CoverPhotoPost");
                    var randomFileName = Path.GetRandomFileName();
                    var fileNameWithExtension = Path.ChangeExtension(randomFileName, ".jpg");
                    postToUpdate.CoverPhoto = fileNameWithExtension;

                    var filePath = string.Concat(path, "/", fileNameWithExtension);
                    await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(postToUpdate);
        }




    }
}
