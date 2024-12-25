using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using React.Models;
using React.share;

namespace React.Migrations
{
    [Route("")]
    [ApiController]
    public class AdminController : Controller
    {
        private readonly SystemDbContext _dbContext;

        public AdminController(SystemDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        [Route("Admins")]
        public async Task<IEnumerable<Admin>> Admins()
        {
            return await _dbContext.Admin.ToListAsync();
        }

        [HttpPost]
        [Route("Admin")]
        public async Task<ActionResult<Admin>> Add([FromForm] AdminCreate objAdmin)
        {
            try
            {
                var existingAdmin = await _dbContext.Admin.FirstOrDefaultAsync(a => a.Name == objAdmin.Name);

                if (existingAdmin != null)
                {
                    return BadRequest("Admin with the same email already exists.");
                }

                var adminEntity = new Admin
                {
                    Email = objAdmin.Email,
                    Name = objAdmin.Name,
                    Password = objAdmin.Password,
                    State = "3"
                };

                _dbContext.Admin.Add(adminEntity);
                await _dbContext.SaveChangesAsync();

         
                return adminEntity;
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Route("Admin")]
        public async Task<ActionResult<Admin>> Delete(int id)
        {
            var adminToDelete = await _dbContext.Admin.FindAsync(id);

            if (adminToDelete == null)
            {
                return NotFound();
            }

            _dbContext.Admin.Remove(adminToDelete);
            await _dbContext.SaveChangesAsync();

            return Ok(adminToDelete);
        }
        [HttpPatch]
        [Route("Admin")]
        public async Task<IActionResult> Edit(int id, [FromForm] AdminCreate updatedAdminData)
        {
            var adminToUpdate = await _dbContext.Admin.FindAsync(id);

            if (adminToUpdate == null)
            {
                return NotFound();
            }

            adminToUpdate.Name = updatedAdminData.Name;
            adminToUpdate.Password = updatedAdminData.Password;

            await _dbContext.SaveChangesAsync();

            return Ok(adminToUpdate);
        }


    }
}
