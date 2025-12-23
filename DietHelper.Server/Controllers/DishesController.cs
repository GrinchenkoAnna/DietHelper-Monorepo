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

        [HttpPost("{userId}/{dishId}/ingredients")]
        public async Task<ActionResult> AddUserDishIngredient(int userId, int dishId, UserDishIngredient userDishIngredient)
        {
            try
            {
                //найти блюдо
                var userDish = await _dbContext.UserDishes
                    .FirstOrDefaultAsync(d => d.UserId == userId && d.Id == dishId && !d.IsDeleted);

                if (userDish is null) return NotFound();

                //добавить связь с блюдом
                userDishIngredient.UserDishId = dishId;
                userDishIngredient.IsDeleted = false;

                //найти соответствующий продукт
                var userProduct = await _dbContext.UserProducts
                    .Include(up => up.BaseProduct)
                    .FirstOrDefaultAsync(up => up.Id == userDishIngredient.UserProductId && !up.IsDeleted);

                //рассчитать кбжу ингредиента по продукту
                userDishIngredient.CalculateNutrition(userProduct);

                //добавить в таблицу
                _dbContext.UserDishIngredients.Add(userDishIngredient);

                //обновить кбжу блюда
                userDish.UpdateNutritionFromIngredients();

                await _dbContext.SaveChangesAsync();
                return Ok(userDishIngredient.Id);
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

        //[HttpPut("{userId}/{userDish}")]
        //public async Task<ActionResult<UserDish>> UpdateUserDish(int userId, UserDish userDish)
        //{
        //    try
        //    {
        //        var dish = await _dbContext.UserDishes
        //            .Include(d => d.Ingredients)
        //            .ThenInclude(i => i.UserProduct)
        //            .ThenInclude(up => up.BaseProduct)
        //            .FirstOrDefaultAsync(d => d.Id == id && d.UserId == userId && !d.IsDeleted);

        //        if (dish == null) return NotFound();
        //        return Ok(dish);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"[DishesController]: {ex.Message}");
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
