using DietHelper.Common.Data;
using DietHelper.Common.Models.Dishes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

        public class AddIngredientRequest
        {
            public int UserProductId { get; set; }
            public double Quantity { get; set; }
        }


        [HttpPost("{userId}/{dishId}/ingredients")]
        public async Task<ActionResult> AddUserDishIngredient(
            int userId,
            int dishId,
            [FromBody] AddIngredientRequest request)
        {
            try
            {
                var userDish = await _dbContext.UserDishes
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == dishId);
                if (userDish is null)
                    return NotFound();

                var userProduct = await _dbContext.UserProducts
                    .Include(up => up.BaseProduct)
                    .FirstOrDefaultAsync(up => up.Id == request.UserProductId && !up.IsDeleted);

                if (userProduct is null)
                    return NotFound($"Product with id {request.UserProductId} not found");

                var existingIngredient = await _dbContext.UserDishIngredients
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(udi => udi.UserDishId == dishId
                        && udi.UserProductId == request.UserProductId);

                existingIngredient.IsDeleted = false;
                existingIngredient.Quantity = request.Quantity;
                existingIngredient.CalculateNutrition(userProduct);

                userDish.UpdateNutritionFromIngredients();

                await _dbContext.SaveChangesAsync();
                return Ok(existingIngredient.Id);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[DishesController]: {ex.Message}");
            }
        }

        [HttpDelete("{userId}/{dishId}/ingredients/{ingredientId}")]
        public async Task<ActionResult> RemoveUserDishIngredient(int userId, int dishId, int ingredientId)
        {
            try
            {
                var userDishIngredient = await _dbContext.UserDishIngredients
                    .FirstOrDefaultAsync(udi => udi.UserDishId == dishId && udi.Id == ingredientId && !udi.IsDeleted);

                if (userDishIngredient is null) return NotFound();

                //удалить
                userDishIngredient.IsDeleted = true;

                //обновить блюдо
                var userDish = await _dbContext.UserDishes
                    .Include(ud => ud.Ingredients)
                    .FirstOrDefaultAsync(ud => ud.UserId == userId && ud.Id == dishId && !ud.IsDeleted);

                if (userDish is not null)
                    userDish.UpdateNutritionFromIngredients();

                await _dbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[DishesController]: {ex.Message}");
            }
        }
    }
}
