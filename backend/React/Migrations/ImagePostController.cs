using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class ImagePostController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public ImagePostController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("ImagePosts")]
        public async Task<IEnumerable<ImagePost>> ImagePost()
        {
            var images = await _dbContext.ImagePost.ToListAsync();

            foreach (var image in images)
            {
                image.Images = AddServerUrlToProfilePic(image.Images);
            }

            return images;
        }

        [HttpGet]
        [Route("ImagePosts/{id}")]
        public async Task<IActionResult> ImagePostById(int id)
        {
            var imagePost = await _dbContext.ImagePost.FirstOrDefaultAsync(post => post.ImagePostId == id);

            if (imagePost == null)
            {
                return NotFound();
            }

            imagePost.Images = AddServerUrlToProfilePic(imagePost.Images);

            return Ok(imagePost);
        }


        private string AddServerUrlToProfilePic(string filename)
        {
            string serverUrl = "http://10.0.2.172/test/ImagePost/";
            return serverUrl + filename;
        }

        [HttpPost]
        [Route("ImagePost")]
        public async Task<ActionResult<List<ImagePost>>> Add([FromForm] ImagePostCreate objPost)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var uploadedImages = new List<ImagePost>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (objPost.Images != null && objPost.Images.Count > 0)
            {
                if (objPost.Images.Count > 6)
                {
                    return BadRequest("You can upload up to 6 image files.");
                }

                // Find ShopId using UserId
                var shopId = await _dbContext.Shop
                    .Where(s => s.UserId == userId)
                    .Select(s => s.ShopId)
                    .FirstOrDefaultAsync();

                if (shopId == 0)
                {
                    return BadRequest("Shop not found for the user.");
                }

                // Find PostId using ShopId
                var postId = await _dbContext.Post
                    .Where(p => p.ShopId == shopId)
                    .Select(p => p.PostId)
                    .FirstOrDefaultAsync();

                if (postId == 0)
                {
                    return BadRequest("Post not found for the user.");
                }

                var existingImagesCount = await _dbContext.ImagePost
                    .Where(ip => ip.PostId == postId)
                    .CountAsync();

                if (existingImagesCount + objPost.Images.Count > 6)
                {
                    return BadRequest("Total images cannot exceed 6 for a single post.");
                }

                foreach (var file in objPost.Images)
                {
                    var fileExtension = Path.GetExtension(file.FileName);

                    if (!allowedExtensions.Contains(fileExtension.ToLower()))
                    {
                        return BadRequest($"Invalid file format for {file.FileName}. Please upload a valid image file.");
                    }

                    var postentity = new ImagePost
                    {
                        PostId = postId
                    };

                    var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ImagePost");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    var randomFileName = Path.GetRandomFileName();
                    var fileNameWithExtension = Path.ChangeExtension(randomFileName, fileExtension);
                    var filePath = Path.Combine(path, fileNameWithExtension);

                    using (FileStream fs = System.IO.File.Create(filePath))
                    {
                        file.CopyTo(fs);
                        fs.Flush();
                    }

                    postentity.Images = fileNameWithExtension;
                    uploadedImages.Add(postentity);
                }
            }

            _dbContext.ImagePost.AddRange(uploadedImages);
            await _dbContext.SaveChangesAsync();

            return uploadedImages;
        }


        [HttpDelete]
        [Route("ImagePost")]
        public async Task<ActionResult<ImagePost>> Delete(int id)
        {
            var ImagePostsToDelete = await _dbContext.ImagePost.FindAsync(id);

            if (ImagePostsToDelete == null)
            {
                return NotFound();
            }

            _dbContext.ImagePost.Remove(ImagePostsToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(ImagePostsToDelete);
        }

        [HttpPatch]
        [Route("ImagePost")]
        public async Task<IActionResult> Edit(int id, [FromForm] ImagePostUpdate objUpdate)
        {
            var existingPost = await _dbContext.ImagePost.FirstOrDefaultAsync(p => p.PostId == id);

            if (existingPost == null)
            {
                return NotFound($"ImagePost with ID {id} not found.");
            }

            if (objUpdate.NewImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(objUpdate.NewImage.FileName);

                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                {
                    return BadRequest("Invalid file format. Please upload a valid image file.");
                }

                var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ImagePost");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                var randomFileName = Path.GetRandomFileName();
                var fileNameWithExtension = Path.ChangeExtension(randomFileName, fileExtension);
                var filePath = Path.Combine(path, fileNameWithExtension);

                using (FileStream fs = System.IO.File.Create(filePath))
                {
                    objUpdate.NewImage.CopyTo(fs);
                    fs.Flush();
                }
                if (!string.IsNullOrEmpty(existingPost.Images))
                {
                    var oldFilePath = Path.Combine(path, existingPost.Images);
                    System.IO.File.Delete(oldFilePath);
                }


                existingPost.Images = fileNameWithExtension;
            }

            await _dbContext.SaveChangesAsync();
            return Ok(existingPost);
        }
    }
}
