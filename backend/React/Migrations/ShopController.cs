using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Claims;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class ShopController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public ShopController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("Shops")]
        public async Task<IEnumerable<ShopWithUserName>> Shop()
        {
            var shops = await _dbContext.Shop
                .Include(s => s.User)
                .Include(s => s.Post)
                .ToListAsync();

            var result = shops.Select(shop =>
            {
                // Check if User is not null before accessing Name and Email
                var userName = shop.User != null ? shop.User.UserName : null;
                var userEmail = shop.User != null ? shop.User.Email : null;

                // Create ShopWithUserName object with necessary data from Shop
                return new ShopWithUserName
                {
                    ShopId = shop.ShopId,
                    ShopName = shop.ShopName,
                    Detail = shop.Detail,
                    UserId = shop.UserId,
                    Email = userEmail,
                    AddressNumber = shop.AddressNumber,
                    District = shop.District,
                    Province = shop.Province,
                    Post = shop.Post,
                    Name = shop.User.Name,
                };
            });

            return result;
        }

        // สร้างคลาส ShopWithUserName เพื่อเก็บข้อมูลที่ต้องการแสดง
        public class ShopWithUserName
        {
            public int ShopId { get; set; }
            public string? ShopName { get; set; }
            public string? Detail { get; set; }
            public int UserId { get; set; }
            public string? Email { get; set; }
            public string? Name { get; set; }
            public string? AddressNumber { get; set; }
            public string? District { get; set; }
            public string? Province { get; set; }
            public Post Post { get; set; }
        }

        [HttpPost]
        [Route("Shop")]
        public async Task<IActionResult> Add([FromForm] ShopCreate objShop)
        {
            try
            {
                if (string.IsNullOrEmpty(objShop.ShopName))
                {
                    return BadRequest("Invalid input parameters.");
                }

            
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Invalid or missing user information in the token.");
                }

                var existingUser = await _dbContext.User.Include(u => u.Shop).FirstOrDefaultAsync(u => u.UserId == userId);

                if (existingUser == null)
                {
                    return BadRequest("User does not exist.");
                }

                if (existingUser.Shop != null)
                {
                    return Conflict("User already has a shop.");
                }

                Shop newShop = new Shop
                {
                    ShopName = objShop.ShopName,
                    AddressNumber = objShop.AddressNumber,
                    Detail = objShop.Detail,
                    District = objShop.District,
                    Province = objShop.Province,
                    UserId = userId, 
                };

                _dbContext.Shop.Add(newShop);
                await _dbContext.SaveChangesAsync();

             
                existingUser = await _dbContext.User.Include(u => u.Shop).FirstOrDefaultAsync(u => u.UserId == userId);

                return Ok(existingUser.Shop);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpDelete]
        [Route("Shop")]
        public async Task<ActionResult<Shop>> Delete()
        {

            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }


            var shopToDelete = await _dbContext.Shop.FirstOrDefaultAsync(s => s.UserId == userId);

            if (shopToDelete == null)
            {
                return NotFound();
            }

            if (shopToDelete.UserId != userId)
            {
                return Forbid("You are not authorized to delete this shop.");
            }

            _dbContext.Shop.Remove(shopToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(shopToDelete);
        }

        [HttpPatch]
        [Route("Shop")]
        public async Task<IActionResult> Edit([FromForm] ShopUpdate updatedShopData)
        {
            // Get UserId from token
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid or missing user information in the token.");
            }

            // Find shop by UserId
            var shopToUpdate = await _dbContext.Shop.Include(s => s.User).FirstOrDefaultAsync(s => s.User.UserId == userId);

            if (shopToUpdate == null)
            {
                return NotFound("Shop not found.");
            }

            // Update shop properties
            shopToUpdate.ShopName = updatedShopData.ShopName;
            shopToUpdate.Detail = updatedShopData.Detail;
            shopToUpdate.AddressNumber = updatedShopData.AddressNumber;
            shopToUpdate.District = updatedShopData.District;
            shopToUpdate.Province = updatedShopData.Province;

            // Save changes to the database
            await _dbContext.SaveChangesAsync();

            return Ok(shopToUpdate);
        }

        [HttpGet]
        [Route("Shop/userId")]
        public async Task<IActionResult> GetShopByUserId(int userId)
        {
            var shop = await _dbContext.Shop
                .Include(s => s.User)
                .Include(s => s.Post)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (shop == null)
            {
                return NotFound();
            }

            var result = new ShopWithUserName
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Detail = shop.Detail,
                UserId = shop.UserId,
                AddressNumber = shop.AddressNumber,
                District = shop.District,
                Province = shop.Province,
                Post = shop.Post,
                Name = shop.User.Name,
            };

            return Ok(result);
        }

        [HttpGet]
        [Route("Shop/{shopId}")]
        public async Task<IActionResult> GetShopByShopId(int shopId)
        {
            // ค้นหาร้านค้าโดยใช้ shopId
            var shop = await _dbContext.Shop
                .Include(s => s.User)
                .Include(s => s.Post)
                .FirstOrDefaultAsync(s => s.ShopId == shopId);

            // ถ้าไม่พบร้านค้า
            if (shop == null)
            {
                return NotFound();
            }

            // สร้าง ShopWithUserName object สำหรับการแสดงผล
            var result = new ShopWithUserName
            {
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                Detail = shop.Detail,
                UserId = shop.UserId,
                AddressNumber = shop.AddressNumber,
                District = shop.District,
                Province = shop.Province,
                Post = shop.Post,
                Name = shop.User.Name,
             
            };

            // ส่งผลลัพธ์กลับให้กับ client
            return Ok(result);
        }



    }
}
