using DietHelper.Common.Data;
using DietHelper.Common.Models.Dishes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public DishesController(DietHelperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("{userId}")]
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
                return StatusCode(500, $"[DishesController]: {ex.Message}");
            }
        }

        [HttpGet("{userId}/{id}")]
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
                return StatusCode(500, $"[DishesController]: {ex.Message}");
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
