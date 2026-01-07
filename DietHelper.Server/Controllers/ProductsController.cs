using DietHelper.Common.Data;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using System.Security.Cryptography.X509Certificates;
using static DietHelper.Server.Controllers.DishesController;

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

        [HttpGet("base")]
        public async Task<ActionResult<List<UserProduct>>> GetBaseProducts()
        {
            try
            {
                var products = await _dbContext.BaseProducts
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[ProductsController]: {ex.Message}");
            }
        }

        [HttpGet("base/{id}")]
        public async Task<ActionResult<BaseProduct>> GetBaseProductById(int id)
        {
            var baseProduct = await _dbContext.BaseProducts
                .FirstOrDefaultAsync(bp => bp.Id == id && !bp.IsDeleted);

            if (baseProduct == null) return NotFound();

            return Ok(baseProduct);
        }


        [HttpPost("user")]
        public async Task<ActionResult> AddUserProduct([FromBody] UserProduct userProduct)
        {
            var baseProduct = await _dbContext.BaseProducts
                .Where(bp => bp.Id == userProduct.BaseProductId && !bp.IsDeleted)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (baseProduct is null)
                return NotFound($"[ProductsController]: BaseProduct with id = {userProduct.BaseProductId} not found");            

            userProduct.IsDeleted = false;

            _dbContext.UserProducts.Add(userProduct);
            await _dbContext.SaveChangesAsync();

            return Ok(userProduct);
        }

        [HttpPost("base")]
        public async Task<ActionResult> AddBaseProduct([FromBody] BaseProduct baseProduct)
        {
            _dbContext.BaseProducts.Add(baseProduct);
            await _dbContext.SaveChangesAsync();

            return Ok(baseProduct);
        }

        

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            Console.WriteLine("[Ping] Получен запрос");
            return Ok("pong");
        }
    }
}
