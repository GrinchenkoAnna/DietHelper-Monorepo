using DietHelper.Common.Data;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Core;
using DietHelper.Common.Models.Dishes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Security.Claims;

namespace DietHelper.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DishesController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public DishesController(DietHelperDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private int GetCurrentUserId()
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userClaim is null || !int.TryParse(userClaim, out int userId))
                throw new UnauthorizedAccessException("User ID not found");

            return userId;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserDish>>> GetUserDishes()
        {
            try
            {
                var userId = GetCurrentUserId();

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

        [HttpGet("{userDishId}")]
        public async Task<ActionResult<UserDish>> GetUserDish(int userDishId)
        {
            try
            {
                var userId = GetCurrentUserId();

                var dish = await _dbContext.UserDishes
                    .Include(ud => ud.Ingredients)
                    .ThenInclude(i => i.UserProduct)
                    .ThenInclude(up => up.BaseProduct)
                    .FirstOrDefaultAsync(ud => ud.Id == userDishId && ud.UserId == userId);

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

        [HttpPost]
        public async Task<ActionResult> AddUserDish([FromBody] UserDish userDish)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (userDish == null) return BadRequest("Request is null");

                userDish.UserId = userId;

                if (string.IsNullOrWhiteSpace(userDish.Name))
                    return BadRequest("Dish name is required");

                userDish.Ingredients ??= new List<UserDishIngredient>();
                userDish.IsReadyDish = userDish.IsReadyDish;
                userDish.IsDeleted = false;

                _dbContext.UserDishes.Add(userDish);
                await _dbContext.SaveChangesAsync();

                return Ok(userDish);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[DishesController]: {ex.Message}");
            }           
        }


        [HttpPost("{dishId}/ingredients")]
        public async Task<ActionResult> AddUserDishIngredient(
            int dishId,
            [FromBody] AddIngredientRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();

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

                if (existingIngredient is null)
                {
                    var newIngredient = new UserDishIngredient
                    {
                        UserDishId = dishId,
                        UserProductId = request.UserProductId,
                        Quantity = request.Quantity,
                        IsDeleted = false
                    };

                    newIngredient.CalculateNutrition(userProduct);
                    _dbContext.UserDishIngredients.Add(newIngredient);

                    userDish.UpdateNutritionFromIngredients();
                    await _dbContext.SaveChangesAsync();

                    return Ok(newIngredient.Id);
                }
                else
                {
                    existingIngredient.IsDeleted = false;
                    existingIngredient.Quantity = request.Quantity;
                    existingIngredient.CalculateNutrition(userProduct);

                    userDish.UpdateNutritionFromIngredients();
                    await _dbContext.SaveChangesAsync();

                    return Ok(existingIngredient.Id);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[DishesController]: {ex.Message}");
            }
        }

        [HttpDelete("{dishId}")]
        public async Task<ActionResult> RemoveUserDish(int dishId)
        {
            try
            {
                int userId = GetCurrentUserId();

                var userDish = await _dbContext.UserDishes
                    .Include(d => d.Ingredients)
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == dishId && !d.IsDeleted);
                if (userDish == null) return NotFound();

                userDish.IsDeleted = true;

                foreach (var ingredient in userDish.Ingredients)
                    ingredient.IsDeleted = true;

                await _dbContext.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"[DishesController]: {ex.Message}");
            }
        }

        [HttpDelete("{dishId}/ingredients/{ingredientId}")]
        public async Task<ActionResult> RemoveUserDishIngredient(int dishId, int ingredientId)
        {
            try
            {
                var userId = GetCurrentUserId();

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
