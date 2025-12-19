using DietHelper.Common.Data;
using DietHelper.Common.Models.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SimpleProductsController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public SimpleProductsController(DietHelperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("mocks")]
        public async Task<ActionResult<List<UserProduct>>> GetMockProducts()
        {
            try
            {
                var products = await _dbContext.UserProducts
                    .Include(p => p.UserId)
                    .Include(p => p.BaseProduct)
                    .Where(p => !p.IsDeleted)
                    .ToListAsync();

                return Ok(products);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("mockProduct/{id}")]
        public async Task<ActionResult<List<UserProduct>>> GetMockProduct(int id)
        {
            try
            {
                var product = await _dbContext.UserProducts
                    .Include(p => p.BaseProduct)
                    .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                if (product == null)
                    return NotFound();

                return Ok(product);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("mockDish/{id}")]
        public async Task<ActionResult<List<UserProduct>>> GetMockDish(int id)
        {
            try
            {
                var dish = await _dbContext.UserDishes
                    .Include(d => d.Ingredients)  
                    .ThenInclude(i => i.UserProduct)
                    .ThenInclude(up => up.BaseProduct)
                    .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

                if (dish == null)
                    return NotFound();

                return Ok(dish);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }
    }
}
