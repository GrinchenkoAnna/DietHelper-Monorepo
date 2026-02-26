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
        public async Task<ActionResult<List<UserMealEntryDto>>> GetUserMeals([FromQuery] DateTime date)
        {
            var userId = GetCurrentUserId();

            var userMeals = await _dbContext.UserMealEntries
                .Include(ume => ume.Ingredients.Where(i => !i.IsDeleted))
                    .ThenInclude(i => i.UserProduct)
                        .ThenInclude(up => up.BaseProduct)
                .Include(ume => ume.UserDish)
                .Where(ume => ume.UserId == userId
                        && ume.Date >= date && ume.Date < date.AddDays(1)
                        && !ume.IsDeleted)
                .ToListAsync();

            var userMealEntryDtos = userMeals.Select(MapModelToDto).ToList();

            return Ok(userMealEntryDtos);
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
        public async Task<ActionResult<UserMealEntryDto>> UpdateUserMeal(int id, UserMealEntryDto request)
        {
            var userId = GetCurrentUserId();

            if (request.Ingredients is null)
                return BadRequest("Meal entry cannot be empty");

            var productIds = request.Ingredients.Select(i => i.UserProductId).Distinct().ToList();
            var existingProductIds = await _dbContext.UserProducts
                .Where(up => productIds.Contains(up.Id) && up.UserId == userId && !up.IsDeleted)
                .Select(up => up.Id)
                .ToListAsync();
            if (productIds.Count != existingProductIds.Count)
            {
                var missingIds = productIds.Except(existingProductIds).ToList();
                return BadRequest($"User Products with ids {string.Join(", ", missingIds)} not found in the user with id = {userId}");
            }

            if (request.UserDishId.HasValue)
            {
                if (request.UserDishId <= 0) return BadRequest("UserDishId cannot be negative");

                var isDishExists = await _dbContext.UserDishes
                    .AnyAsync(ud => ud.Id == request.UserDishId && ud.UserId == userId && !ud.IsDeleted);
                if (!isDishExists) return BadRequest($"User Dish with id {request.UserDishId} not found in the user with id = {userId}");
            }

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

            return Ok(MapModelToDto(userMealEntry));
        }

        [HttpPost]
        public async Task<ActionResult<UserMealEntryDto>> AddUserMeal(UserMealEntryDto request)
        {
            int userId = GetCurrentUserId();

            if (request is null || request.Ingredients is null)
                return BadRequest("Meal entry cannot be empty");

            var productIds = request.Ingredients.Select(i => i.UserProductId).Distinct().ToList();
            var existingProductIds = await _dbContext.UserProducts
                .Where(up => productIds.Contains(up.Id) && up.UserId == userId && !up.IsDeleted)
                .Select(up => up.Id)
                .ToListAsync();
            if (productIds.Count != existingProductIds.Count)
            {
                var missingIds = productIds.Except(existingProductIds).ToList();
                return BadRequest($"User Products with ids {string.Join(", ", missingIds)} not found in the user with id = {userId}");
            }

            if (request.UserDishId.HasValue)
            {
                if (request.UserDishId <= 0) return BadRequest("UserDishId cannot be negative");

                var isDishExists = await _dbContext.UserDishes
                    .AnyAsync(ud => ud.Id == request.UserDishId && ud.UserId == userId && !ud.IsDeleted);
                if (!isDishExists) return BadRequest($"User Dish with id {request.UserDishId} not found in the user with id = {userId}");
            }

            var userMealEntry = new UserMealEntry()
            {
                UserId = userId,
                UserDishId = request.UserDishId,
                Date = request.Date,
                CreatedAt = DateTime.UtcNow,
            };

            if (request.Ingredients.Count > 0)
                userMealEntry = await AddIngredientsToUserMealEntry(userMealEntry, request);
            else
            {
                userMealEntry.TotalNutrition = request.TotalNutrition;
                userMealEntry.TotalQuantity = request.TotalQuantity;
            }

            _dbContext.UserMealEntries.Add(userMealEntry);
            await _dbContext.SaveChangesAsync();

            // полная модель со всеми связями
            userMealEntry = await GetUserMeal(userMealEntry.Id);

            return Ok(MapModelToDto(userMealEntry));
        }

        private UserMealEntryDto MapModelToDto(UserMealEntry userMealEntry)
        {
            return new UserMealEntryDto()
            {
                Id = userMealEntry.Id,
                Date = userMealEntry.Date,
                UserDishId = userMealEntry.UserDishId,
                UserDishName = userMealEntry.UserDish?.Name,
                TotalQuantity = userMealEntry.TotalQuantity,
                TotalNutrition = userMealEntry.TotalNutrition,
                Ingredients = userMealEntry.Ingredients.Select(i => new UserMealEntryIngredientDto()
                {
                    Id = i.Id,
                    UserProductId = i.UserProductId,
                    Quantity = i.Quantity,
                    ProductNameSnapshot = i.ProductNameSnapshot,
                    ProductNutritionInfoSnapshot = i.ProductNutritionInfoSnapshot,
                }).ToList()
            };
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
