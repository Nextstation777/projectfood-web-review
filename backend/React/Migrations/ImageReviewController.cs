using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class ImageReviewController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public ImageReviewController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("ImageReviews")]
        public async Task<IEnumerable<ImageReview>> ImageReview()
        {
            var images = await _dbContext.ImageReview.ToListAsync();

            foreach (var image in images)
            {
                image.Images = AddServerUrlToProfilePic(image.Images);
            }

            return images;
        }

        [HttpGet]
        [Route("ImageReview/{id}")]
        public async Task<IActionResult> ImageReviewById(int id)
        {
            var imageReview = await _dbContext.ImageReview.FirstOrDefaultAsync(review => review.ImageReviewId == id);

            if (imageReview == null)
            {
                return NotFound();
            }

            imageReview.Images = AddServerUrlToProfilePic(imageReview.Images);

            return Ok(imageReview);
        }

        private string AddServerUrlToProfilePic(string filename)
        {
            string serverUrl = "http://10.0.2.172/test/ImageReview/";
            return serverUrl + filename;
        }

        [HttpPost]
        [Route("ImageReview")]
        public async Task<ActionResult<List<ImageReview>>> Add([FromForm] ImageReviewCreate objR)
        {

            var uploadedImages = new List<ImageReview>();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

            if (objR.Images != null && objR.Images.Count > 0)
            {
            
                if (objR.Images.Count > 6)
                {
                    return BadRequest("You can upload up to 6 image files.");
                }

     
                var existingImagesCount = await _dbContext.ImageReview
                    .Where(ip => ip.ReviewId == objR.ReviewId)
                    .CountAsync();

      
                if (existingImagesCount + objR.Images.Count > 6)
                {
                    return BadRequest("Total images cannot exceed 6 for a single post.");
                }

                foreach (var file in objR.Images)
                {
                    var fileExtension = Path.GetExtension(file.FileName);

                    if (!allowedExtensions.Contains(fileExtension.ToLower()))
                    {
                        return BadRequest($"Invalid file format for {file.FileName}. Please upload a valid image file.");
                    }

                    var reviewentity = new ImageReview
                    {
                        ReviewId = objR.ReviewId
                    };

                    var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ImageReview");
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

                    reviewentity.Images = fileNameWithExtension;
                    uploadedImages.Add(reviewentity);
                }
            }

            _dbContext.ImageReview.AddRange(uploadedImages);
            await _dbContext.SaveChangesAsync();

            return uploadedImages;
        }
        [HttpDelete]
        [Route("ImageReview")]
        public async Task<ActionResult<ImageReview>> Delete(int id)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var imageReviewToDelete = await _dbContext.ImageReview.FindAsync(id);

            if (imageReviewToDelete == null)
            {
                return NotFound();
            }

            // Find the owner of the image review
            var owner = await _dbContext.Review.FindAsync(imageReviewToDelete.ReviewId);

            // Check if the user owns the image review before deleting
            if (owner.UserId != userId)
            {
                return Forbid("You are not authorized to delete this image review.");
            }

            _dbContext.ImageReview.Remove(imageReviewToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(imageReviewToDelete);
        }


        [HttpPatch]
        [Route("ImageReview")]
        public async Task<IActionResult> Edit(int id, [FromForm] ImageReviewUpdate objUpdate)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var existingReview = await _dbContext.ImageReview.FirstOrDefaultAsync(p => p.ReviewId == id);

            if (existingReview == null)
            {
                return NotFound($"ImageReview with ID {id} not found.");
            }

            // Find the owner of the image review
            var owner = await _dbContext.Review.FindAsync(existingReview.ReviewId);

            // Check if the user owns the image review before editing
            if (owner.UserId != userId)
            {
                return Forbid("You are not authorized to edit this image review.");
            }

            if (objUpdate.NewImage != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                var fileExtension = Path.GetExtension(objUpdate.NewImage.FileName);

                if (!allowedExtensions.Contains(fileExtension.ToLower()))
                {
                    return BadRequest("Invalid file format. Please upload a valid image file.");
                }

                var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\ImageReview");
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
                if (!string.IsNullOrEmpty(existingReview.Images))
                {
                    var oldFilePath = Path.Combine(path, existingReview.Images);
                    System.IO.File.Delete(oldFilePath);
                }

                existingReview.Images = fileNameWithExtension;
            }

            await _dbContext.SaveChangesAsync();
            return Ok(existingReview);
        }

    }
}
