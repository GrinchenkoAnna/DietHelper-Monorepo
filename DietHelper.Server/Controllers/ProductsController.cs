using DietHelper.Common.Data;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public ProductsController(DietHelperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<List<UserProduct>>> GetUserProducts(int userId)
        {
            try
            {
                var products = await _dbContext.UserProducts
                    .Include(p => p.BaseProduct)
                    .Where(p => p.UserId == userId && !p.IsDeleted)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[ProductsController]: {ex.Message}");
            }
        }

        [HttpGet("{userId}/{id}")]
        public async Task<ActionResult<UserProduct>> GetUserProduct(int userId, int id)
        {
            try
            {
                var product = await _dbContext.UserProducts
                    .Include(p => p.BaseProduct)
                    .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId && !p.IsDeleted);

                if (product == null) return NotFound();
                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[ProductsController]: {ex.Message}");
            }
        }

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            Console.WriteLine("[Ping] Получен запрос");
            return Ok("pong");
        }
    }
}
