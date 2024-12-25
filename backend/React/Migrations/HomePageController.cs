using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using React.Models;
using React.share;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;


namespace React.Migrations
{
    [ApiController]
    [Route("")]
    public class HomePageController : ControllerBase
    {
        private readonly SystemDbContext _dbContext;

        public HomePageController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("Search")]
        public async Task<IActionResult> Search(string keyword)
        {
            try
            {
                var shops = await _dbContext.Shop
                    .Where(s => s.ShopName.Contains(keyword) || s.Province.Contains(keyword) || s.District.Contains(keyword))
                    .ToListAsync();

                var posts = await _dbContext.Post
                    .Where(p => p.Topic.Contains(keyword))
                    .Select(p => new
                    {
                        Post = p,
                        CoverPhoto = $"http://10.0.2.172/test/CoverPhotoPost/{p.CoverPhoto}"
                    })
                    .ToListAsync();

                var reviews = await _dbContext.Review
                    .Where(r => r.Topic.Contains(keyword))
                    .Select(r => new
                    {
                        Review = r,
                        CoverPhoto = $"http://10.0.2.172/test/CoverPhotoReview/{r.CoverPhoto}"
                    })
                    .ToListAsync();

                var distinctShops = shops.GroupBy(s => s.ShopId).Select(g => g.First());
        
                posts.ForEach(p => p.Post.CoverPhoto = p.CoverPhoto);
                reviews.ForEach(r => r.Review.CoverPhoto = r.CoverPhoto);

                var result = new
                {
                    Shops = distinctShops,
                    Posts = posts.Select(p => p.Post),
                    Reviews = reviews.Select(r => r.Review)
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("Recomment")]
        public async Task<IEnumerable<PostWithShop2>> Recomment()
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
                    UserIds = p.RatingPost.Select(rp => rp.UserId).ToList(),
                    CommentCount = p.CommentPost.Count(),
                })
                .ToListAsync();

            var result = posts.Select(post =>
            {
                var imagePosts = post.ImagePost?.Select(ImagePost => $"https://localhost:7199/ImagePost/{ImagePost?.Images}").ToList();

                double recom = (post.UserIds.Count * 0.6) + (post.PostScore * 0.4) + (post.CommentCount * 0.2);

                return new PostWithShop2
                {
                    PostId = post.PostId,
                    Topic = post.Topic,
                    Text = post.Text,
                    CreateTime = post.CreateTime,
                    ShopName = post.ShopName,
                    ShopId = post.ShopId,
                    Shop = post.Shop,
                    PostScore = post.PostScore,
                    Recom = recom,
                    CoverPhotoUrl = GetCoverPhotoUrl(post.CoverPhoto),
                    ImagePost = imagePosts,
                };
            })
            .OrderByDescending(post => post.Recom)
            .Take(5); // เลือกมาเฉพาะ 5 ตัวแรกที่มีคะแนน Recom มากสุด

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
                return 0;
            }

            int totalScore = ratingPosts.Sum(rp => rp.PostScore);
            int averageScore = totalScore / ratingPosts.Count;

            return averageScore;
        }

        public class PostWithShop2
        {
            public int PostId { get; set; }
            public string? ShopName { get; set; }
            public int ShopId { get; set; }
            public string? Text { get; set; }
            public string? Topic { get; set; }
            public string? CreateTime { get; set; }
            public string? CoverPhotoUrl { get; set; }
            public int PostScore { get; set; }
            public double Recom { get; set; }

            public Shop Shop { get; set; }
            public List<string?> ImagePost { get; set; }
        }

        [HttpGet]
        [Route("RandomReviews")]
        public async Task<IEnumerable<Review>> RandomReviewsWithAverageRating()
        {
            var reviews = await _dbContext.Review
                .Include(u => u.User)
                .Include(s => s.Shop)
                .Include(s => s.RatingReview)
                .ToListAsync();

            var random = new Random();
            var randomReviews = reviews.OrderBy(x => random.Next()).Take(5);

            var result = randomReviews.Select(review =>
            {
                review.CoverPhoto = $"https://localhost:7199/CoverPhotoReview/{review.CoverPhoto}";
                if (review.RatingReview != null && review.RatingReview.Any())
                {
                    double averageRating = review.RatingReview.Average(r => r.ReviewScore);
                    review.ReviewScore = averageRating;           
                }
                else
                {
                    review.ReviewScore = 0;
                }

                return review;
            });

            return result;
        }


        [HttpGet]
        [Route("Test")]
        public async Task<IEnumerable<PostWithShoptest>> Test()
        {
            var posts = await _dbContext.Post
                .Include(p => p.Shop)
                .Include(p => p.ImagePost)
                .Include(p => p.RatingPost)
                .Select(p => new
                {
                    Id = p.PostId,
                    Topic = p.Topic,
                    Text = p.Text,
                    CreateTime = p.CreateTime,
                    CoverPhoto = p.CoverPhoto,
                    ShopId = p.ShopId,
                    Shop = p.Shop,
                    ShopName = p.Shop.ShopName,
                    ImagePost = p.ImagePost,
                    PostScore = CalculateAveragePostScore2(p.RatingPost),
                })
                .ToListAsync();

            var reviews = await _dbContext.Review
                .Include(r => r.Shop)
                .Include(r => r.ImageReview)
                .Include(r => r.RatingReview)
                .Select(r => new
                {
                    Id = r.ReviewId,
                    Topic = r.Topic,
                    Text = r.Text,
                    CreateTime = r.CreateTime,
                    CoverPhoto = r.CoverPhoto,
                    ShopId = r.ShopId,
                    Shop = r.Shop,
                    ShopName = r.Shop.ShopName,
                    ImageReview = r.ImageReview,
                    ReviewScore = CalculateAverageReviewScore(r.RatingReview),
                })
                .ToListAsync();

            var allPosts = posts
    .Select(post => new PostWithShoptest
    {
        PostId = post.Id,
        Topic = post.Topic,
        Text = post.Text,
        CreateTime = post.CreateTime,
        ShopName = post.ShopName,
        ShopId = post.ShopId,
        Shop = post.Shop,
        PostScore = post.PostScore,
        ReviewScore = 0, // Set ReviewScore to 0 for posts
        CoverPhotoUrl = GetCoverPhotoUrl2(post.CoverPhoto, isReview: false), // ใช้ GetCoverPhotoUrl ในการสร้าง URL สำหรับ CoverPhoto โดยกำหนดให้ isReview เป็น false
        ImagePost = post.ImagePost?.Select(image => $"https://localhost:7199/ImagePost/{image?.Images}").ToList(),
        ImageReview = new List<string?>(), // Initialize ImageReview as an empty list for posts
    })
    .Concat(reviews
        .Select(review => new PostWithShoptest
        {
            PostId = review.Id,
            Topic = review.Topic,
            Text = review.Text,
            CreateTime = review.CreateTime,
            ShopName = review.ShopName,
            ShopId = review.ShopId ?? 0,
            Shop = review.Shop,
            PostScore = 0, // Set PostScore to 0 for reviews
            ReviewScore = review.ReviewScore,
            CoverPhotoUrl = GetCoverPhotoUrl2(review.CoverPhoto, isReview: true), // ใช้ GetCoverPhotoUrl ในการสร้าง URL สำหรับ CoverPhoto โดยกำหนดให้ isReview เป็น true
            ImagePost = new List<string?>(), // Initialize ImagePost as an empty list for reviews
            ImageReview = review.ImageReview?.Select(image => $"https://localhost:7199/ImageReview/{image?.Images}").ToList(),
        }))
    .OrderByDescending(item => item.CreateTime)
    .ToList();


            return allPosts;
        }

        private string GetCoverPhotoUrl2(string? coverPhoto, bool isReview)
        {
            if (string.IsNullOrEmpty(coverPhoto))
            {
                return string.Empty;
            }

            string prefix = isReview ? "https://localhost:7199/CoverPhotoReview/" : "https://localhost:7199/CoverPhotoPost/";
            return $"{prefix}{coverPhoto}";
        }

        private static int CalculateAveragePostScore2(List<RatingPost>? ratingPosts)
        {
            if (ratingPosts == null || ratingPosts.Count == 0)
            {
                return 0; // Return 0 if there are no ratings
            }

            int totalScore = ratingPosts.Sum(rp => rp.PostScore);
            int averageScore = totalScore / ratingPosts.Count;

            return averageScore;
        }

        private static int CalculateAverageReviewScore(List<RatingReview>? ratingReviews)
        {
            if (ratingReviews == null || ratingReviews.Count == 0)
            {
                return 0; // Return 0 if there are no ratings
            }

            int totalScore = ratingReviews.Sum(rp => rp.ReviewScore);
            int averageScore = totalScore / ratingReviews.Count;

            return averageScore;
        }


        public class PostWithShoptest
        {
            public int PostId { get; set; }
            public string? ShopName { get; set; }
            public int ShopId { get; set; }
            public string? Text { get; set; }
            public string? Topic { get; set; }
            public string? CreateTime { get; set; }
            public string? CoverPhotoUrl { get; set; }
            public int PostScore { get; set; }
            public int ReviewScore { get; set; } // เพิ่ม ReviewScore
            public Shop Shop { get; set; }
            public List<string?> ImagePost { get; set; }
            public List<string?> ImageReview { get; set; } // เพิ่ม ImageReview
        }


    }
}
