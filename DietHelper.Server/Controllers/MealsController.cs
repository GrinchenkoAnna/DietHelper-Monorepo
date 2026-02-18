using DietHelper.Common.Data;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.MealEntries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DietHelper.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MealsController : Controller
    {
        private readonly DietHelperDbContext _dbContext;

        public MealsController(DietHelperDbContext dbContext)
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
        public async Task<ActionResult<List<UserMealEntry>>> GetUserMeals([FromQuery] DateTime date)
        {
            var userId = GetCurrentUserId();

            var entries = await _dbContext.UserMealEntries
                .Include(ume => ume.Ingredients)
                    .ThenInclude(i => i.UserProduct)
                        .ThenInclude(up => up.BaseProduct)
                .Include(ume => ume.UserDish)
                .Where(ume => ume.UserId == userId
                        && ume.Date >= date && ume.Date < date.AddDays(1)
                        && !ume.IsDeleted)
                .ToListAsync();

            return Ok(entries);
        }
        

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserMeal(int id, UserMealEntryDto request)
        {
            


        }

        [HttpPost]
        public async Task<ActionResult<UserMealEntry>> AddUserMeal(UserMealEntryDto request)
        {
            int userId = GetCurrentUserId();

            if (request is null || request.Ingredients.Count == 0)
                return BadRequest("Meal entry cannot be empty");

            var userMealEntry = new UserMealEntry()
            {
                UserId = userId,
                UserDishId = request.UserDishId,
                Date = request.Date,
                CreatedAt = DateTime.Now,
            };

            foreach (var ingredientDto in request.Ingredients)
            {
                var ingredient = new UserMealEntryIngredient()
                {
                    UserProductId = ingredientDto.UserProductId,
                    Quantity = ingredientDto.Quantity,
                    ProductNameSnapshot = ingredientDto.ProductNameSnapshot,
                    ProductNutritionInfoSnapshot = ingredientDto.ProductNutritionInfoSnapshot
                };
                userMealEntry.Ingredients.Add(ingredient);
            }

            // посчитать граммовки

            _dbContext.UserMealEntries.Add(userMealEntry);
            await _dbContext.SaveChangesAsync();

            userMealEntry = await _dbContext.UserMealEntries
                .Include(ume => ume.Ingredients)
                    .ThenInclude(i => i.UserProduct)
                        .ThenInclude(up => up.BaseProduct)
                .Include(ume => ume.UserDish)
                .FirstOrDefaultAsync(ume => ume.Id == userMealEntry.Id && !ume.IsDeleted);

            return Ok(userMealEntry);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserMeal(int id)
        {

        }
    }
}
