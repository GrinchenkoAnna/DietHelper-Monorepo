using DietHelper.Common.Data;
using DietHelper.Common.DTO;
using DietHelper.Common.Models.Core;
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
                .Include(ume => ume.Ingredients.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.UserProduct)
                        .ThenInclude(up => up.BaseProduct)
                .Include(ume => ume.UserDish)
                .Where(ume => ume.UserId == userId
                        && ume.Date >= date && ume.Date < date.AddDays(1)
                        && !ume.IsDeleted)
                .ToListAsync();

            return Ok(entries);
        }

        private async Task<UserMealEntry?> GetUserMeal(int id)
        {
            var userId = GetCurrentUserId();

            return await _dbContext.UserMealEntries
                .Include(ume => ume.Ingredients.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.UserProduct)
                        .ThenInclude(up => up.BaseProduct)
                .Include(ume => ume.UserDish)
                .FirstOrDefaultAsync(ume => ume.UserId == userId && ume.Id == id && !ume.IsDeleted);
        }

        private async Task<UserMealEntry> AddIngredientsToUserMealEntry(UserMealEntry userMealEntry, UserMealEntryDto request)
        {
            decimal totalQuantity = 0;
            var totalNutrition = new NutritionInfo();

            foreach (var ingredientDto in request.Ingredients)
            {
                var ingredientNutritionInfo = ingredientDto.ProductNutritionInfoSnapshot;
                var ingredientQuantity = ingredientDto.Quantity;

                var ingredient = new UserMealEntryIngredient()
                {
                    UserProductId = ingredientDto.UserProductId,
                    Quantity = ingredientQuantity,
                    ProductNameSnapshot = ingredientDto.ProductNameSnapshot,
                    ProductNutritionInfoSnapshot = ingredientNutritionInfo
                };
                userMealEntry.Ingredients.Add(ingredient);

                totalQuantity += ingredientDto.Quantity;

                var factor = (double)ingredientQuantity / 100;
                totalNutrition.Calories += ingredientNutritionInfo.Calories * factor;
                totalNutrition.Protein += ingredientNutritionInfo.Protein * factor;
                totalNutrition.Fat += ingredientNutritionInfo.Fat * factor;
                totalNutrition.Carbs += ingredientNutritionInfo.Carbs * factor;
            }

            userMealEntry.TotalQuantity = totalQuantity;
            userMealEntry.TotalNutrition = totalNutrition;

            return userMealEntry;
        }
        

        [HttpPut("{id}")]
        public async Task<ActionResult<UserMealEntry>> UpdateUserMeal(int id, UserMealEntryDto request)
        {
            var userId = GetCurrentUserId();

            if (request.Ingredients is null || request.Ingredients.Count == 0)
                return BadRequest("Meal entry cannot be empty");

            var userMealEntry = await GetUserMeal(id);
            if (userMealEntry is null) return NotFound();

            foreach (var ingredient in userMealEntry.Ingredients)
                ingredient.IsDeleted = true;
            userMealEntry = await AddIngredientsToUserMealEntry(userMealEntry, request);

            userMealEntry.Date = request.Date;
            userMealEntry.UserDishId = request.UserDishId;

            await _dbContext.SaveChangesAsync();

            // полная модель со всеми связями
            userMealEntry = await GetUserMeal(userMealEntry.Id);

            return Ok(userMealEntry);
        }

        [HttpPost]
        public async Task<ActionResult<UserMealEntry>> AddUserMeal(UserMealEntryDto request)
        {
            int userId = GetCurrentUserId();

            if (request is null || request.Ingredients is null || request.Ingredients.Count == 0)
                return BadRequest("Meal entry cannot be empty");
            
            var userMealEntry = new UserMealEntry()
            {
                UserId = userId,
                UserDishId = request.UserDishId,
                Date = request.Date,
                CreatedAt = DateTime.Now,
            };

            userMealEntry = await AddIngredientsToUserMealEntry(userMealEntry, request);

            _dbContext.UserMealEntries.Add(userMealEntry);
            await _dbContext.SaveChangesAsync();

            // полная модель со всеми связями
            userMealEntry = await GetUserMeal(userMealEntry.Id);

            return Ok(userMealEntry);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserMeal(int id)
        {
            var userId = GetCurrentUserId();

            var userMealEntry = await _dbContext.UserMealEntries
                .Include(ume => ume.Ingredients)
                .FirstOrDefaultAsync(ume => ume.Id == id && ume.UserId == userId && !ume.IsDeleted);
            if (userMealEntry is null) return NotFound();

            userMealEntry.IsDeleted = true;
            foreach (var ingredient in userMealEntry.Ingredients)
                ingredient.IsDeleted = true;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}
