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
    public class SimpleProductsController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public SimpleProductsController(DietHelperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{userId}/products")]
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
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("{userId}/products/{id}")]
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
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("{userId}/dishes")]
        public async Task<ActionResult<List<UserDish>>> GetUserDishes(int userId)
        {
            try
            {
                var dishes = await _dbContext.UserDishes
                    .Include(d => d.Ingredients)
                    .ThenInclude(i => i.UserProduct)
                    .ThenInclude(up => up.BaseProduct)
                    .Where(d => d.UserId == userId && !d.IsDeleted)
                    .ToListAsync();

                return Ok(dishes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        [HttpGet("{userId}/dishes/{id}")]
        public async Task<ActionResult<UserDish>> GetUserDish(int userId, int id)
        {
            try
            {
                var dish = await _dbContext.UserDishes
                    .Include(d => d.Ingredients)
                    .ThenInclude(i => i.UserProduct)
                    .ThenInclude(up => up.BaseProduct)
                    .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId && !d.IsDeleted);

                if (dish == null) return NotFound();
                return Ok(dish);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        //[HttpGet("mocks")]
        //public async Task<ActionResult<List<UserProduct>>> GetMockProducts()
        //{
        //    try
        //    {
        //        var products = await _dbContext.UserProducts
        //            .Include(p => p.BaseProduct)
        //            .Where(p => p.UserId == userId &&!p.IsDeleted)
        //            .ToListAsync();

        //        return Ok(products);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Ошибка: {ex.Message}");
        //    }
        //}

        //[HttpGet("mockProduct/{id}")]
        //public async Task<ActionResult<UserProduct>> GetMockProduct(int id)
        //{
        //    try
        //    {
        //        var product = await _dbContext.UserProducts
        //            .Include(p => p.BaseProduct)
        //            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        //        if (product == null)
        //            return NotFound();

        //        return Ok(product);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Ошибка: {ex.Message}");
        //    }
        //}

        //[HttpGet("mockDish/{id}")]
        //public async Task<ActionResult<List<UserProduct>>> GetMockDish(int id)
        //{
        //    try
        //    {
        //        var dish = await _dbContext.UserDishes
        //            .Include(d => d.Ingredients)  
        //            .ThenInclude(i => i.UserProduct)
        //            .ThenInclude(up => up.BaseProduct)
        //            .FirstOrDefaultAsync(d => d.Id == id && !d.IsDeleted);

        //        if (dish == null)
        //            return NotFound();

        //        return Ok(dish);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Ошибка: {ex.Message}");
        //    }
        //}

        [HttpGet("ping")]
        public ActionResult<string> Ping()
        {
            Console.WriteLine("[Ping] Получен запрос");
            return Ok("pong");
        }
    }
}
