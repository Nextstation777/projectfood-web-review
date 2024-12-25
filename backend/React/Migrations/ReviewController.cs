using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class ReviewController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public ReviewController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("Reviews")]
        public async Task<IEnumerable<ReveiwR>> Review()
        {
            var review = await _dbContext.Review
                .Include(p => p.Shop)
                .Include(p => p.ImageReview)
                .Include(p => p.RatingReview)
                .Select(p => new
                {
                    p.ReviewId,
                    p.Topic,
                    p.Text,
                    p.CreateTime,
                    p.CoverPhoto,
                    p.ShopId,
                    p.Shop,
                    p.UserId,
                    ShopName = p.Shop.ShopName,
                    ImageReview = p.ImageReview,
                    ReviewScore = CalculateAverageReviewScore(p.RatingReview),
                })
                .ToListAsync();

            var result = review.Select(review =>
            {
                var imageReviews = review.ImageReview?.Select(ImageReview => $"https://localhost:7199/ImageReview/{ImageReview?.Images}").ToList();
                return new ReveiwR
                {
                    ReviewId = review.ReviewId,
                    Topic = review.Topic,
                    Text = review.Text,
                    CreateTime = review.CreateTime, // Format the DateTime
                    ShopName = review.ShopName,
                    ShopId = review.ShopId,
                    Shop = review.Shop,
                    UserId = review.UserId,
                    ReviewScore = review.ReviewScore,
                    CoverPhotoUrl = GetCoverPhotoUrl(review.CoverPhoto),
                    ImageReview = imageReviews,
                };
            });

            return result;
        }

        private string GetCoverPhotoUrl(string? coverPhoto)
        {
            return string.IsNullOrEmpty(coverPhoto) ? string.Empty : $"https://localhost:7199/CoverPhotoReview/{coverPhoto}";
        }

        private static int CalculateAverageReviewScore(List<RatingReview>? ratingReviews)
        {
            if (ratingReviews == null || ratingReviews.Count == 0)
            {
                return 0; 
            }

            int totalScore = ratingReviews.Sum(rp => rp.ReviewScore);
            int averageScore = totalScore / ratingReviews.Count;

            return averageScore;
        }

        public class ReveiwR
        {
            public int ReviewId { get; set; }
            public string? ShopName { get; set; }
            public int? ShopId { get; set; }
            public string? Text { get; set; }
            public string? Topic { get; set; }
            public string? CreateTime { get; set; }
            public string? CoverPhotoUrl { get; set; }
            public int ReviewScore { get; set; }
            public int UserId { get; set; }

            public Shop Shop { get; set; }
            public List<string?> ImageReview { get; set; }
        }

        [HttpGet]
        [Route("Reviews/{reviewId}")]
        public async Task<IActionResult> ReviewById(int reviewId)
        {
            var review = await _dbContext.Review
                .Include(p => p.Shop)
                .Include(p => p.ImageReview)
                .Include(p => p.RatingReview)
                .Where(p => p.ReviewId == reviewId)
                .Select(p => new
                {
                    p.ReviewId,
                    p.Topic,
                    p.Text,
                    p.CreateTime,
                    p.CoverPhoto,
                    p.ShopId,
                    p.Shop,
                    p.UserId,
                    ShopName = p.Shop.ShopName,
                    ImageReview = p.ImageReview,
                    ReviewScore = CalculateAverageReviewScore(p.RatingReview),
                })
                .FirstOrDefaultAsync();

            if (review == null)
            {
                return NotFound();
            }

            var imageReviews = review.ImageReview?.Select(ImageReview => $"https://localhost:7199/ImageReview/{ImageReview?.Images}").ToList();

            var result = new ReveiwR
            {
                ReviewId = review.ReviewId,
                Topic = review.Topic,
                Text = review.Text,
                CreateTime = review.CreateTime, // Format the DateTime
                ShopName = review.ShopName,
                ShopId = review.ShopId,
                Shop = review.Shop,
                UserId = review.UserId,
                ReviewScore = review.ReviewScore,
                CoverPhotoUrl = GetCoverPhotoUrl(review.CoverPhoto),
                ImageReview = imageReviews,
            };

            return Ok(result);
        }


        [HttpPost]
        [Route("Review")]
        public async Task<ActionResult<Review>> Add([FromForm] ReviewCreate objReview)
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            try
            {
                var reviewEntity = new Review
                {
                    Topic = objReview.Topic,
                    Text = objReview.Text,
                    ShopId = objReview.ShopId,
                    UserId = userId
                };

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };

                if (objReview.CoverPhoto != null)
                {
                    var fileExtension = Path.GetExtension(objReview.CoverPhoto.FileName);

                    if (!allowedExtensions.Contains(fileExtension.ToLower()))
                    {
                        return BadRequest("Invalid file format. Please upload a valid image file.");
                    }

                    var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\CoverPhotoReview");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    var randomFileName = Path.GetRandomFileName();
                    var fileNameWithExtension = Path.ChangeExtension(randomFileName, fileExtension);
                    var filePath = Path.Combine(path, fileNameWithExtension);

                    using (FileStream fs = System.IO.File.Create(filePath))
                    {
                        objReview.CoverPhoto.CopyTo(fs);
                        fs.Flush();
                    }

                    var thaiTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                    reviewEntity.CreateTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, thaiTimeZone).ToString("yyyy-MM-dd HH:mm:ss");

                    reviewEntity.CoverPhoto = fileNameWithExtension;
                }

                _dbContext.Review.Add(reviewEntity);
                await _dbContext.SaveChangesAsync();

                return reviewEntity;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpDelete]
        [Route("Review")]
        public async Task<ActionResult<Review>> Delete(int id)
        {
            
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var reviewToDelete = await _dbContext.Review.FindAsync(id);

            if (reviewToDelete == null)
            {
                return NotFound();
            }

            if (reviewToDelete.UserId != userId)
            {
                return Forbid("You are not authorized to delete this review.");
            }

            _dbContext.Review.Remove(reviewToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(reviewToDelete);
        }

        [HttpPatch]
        [Route("Review")]
        public async Task<IActionResult> Edit(int id, [FromForm] ReviewUpdate updatedReview)
        {
          
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            var reviewToUpdate = await _dbContext.Review.FindAsync(id);

            if (reviewToUpdate == null)
            {
                return NotFound();
            }


            if (reviewToUpdate.UserId != userId)
            {
                return Forbid("You are not authorized to edit this review.");
            }

            reviewToUpdate.Topic = updatedReview.Topic;
            reviewToUpdate.Text = updatedReview.Text;

            if (updatedReview.CoverPhoto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await updatedReview.CoverPhoto.CopyToAsync(memoryStream);

                    if (!string.IsNullOrEmpty(reviewToUpdate.CoverPhoto))
                    {
                        var oldFilePath = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\CoverPhotoReview", reviewToUpdate.CoverPhoto);
                        System.IO.File.Delete(oldFilePath);
                    }

                    var path = Path.Combine("C:\\Chetsada\\test\\React\\React\\wwwroot\\CoverPhotoReview");
                    var randomFileName = Path.GetRandomFileName();
                    var fileNameWithExtension = Path.ChangeExtension(randomFileName, ".jpg");
                    reviewToUpdate.CoverPhoto = fileNameWithExtension;

                    var filePath = Path.Combine(path, fileNameWithExtension);
                    await System.IO.File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
                }
            }

            await _dbContext.SaveChangesAsync();

            return Ok(reviewToUpdate);
        }



    }
}
