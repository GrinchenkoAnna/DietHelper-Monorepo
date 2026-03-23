using DietHelper.Common.DTO;
using DietHelper.Common.Models;
using DietHelper.Common.Models.Dishes;
using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public interface IApiService
    {
        bool IsAuthenticated { get; }

        event Action? AuthStateChanged;

        Task<BaseProduct> AddProductAsync<BaseProduct>(BaseProduct newBaseProduct);
        Task<UserDish?> AddUserDishAsync(UserDish newUserDish);
        Task<int?> AddUserDishIngredientAsync(int dishId, UserDishIngredient userDishIngredient);
        Task<UserMealEntryDto?> AddUserMealEntryAsync(UserMealEntryDto request);
        Task<UserProduct> AddUserProductAsync(UserProduct newUserProduct);
        Task<bool> CheckServerConnectionAsync();
        Task DeleteDishAsync(int id);
        Task<bool> DeleteUserMealEntryAsync(int userMealEntryId);
        Task DeleteUserProductAsync(int id);
        Task<List<BaseProduct>?> GetBaseProductsAsync();
        Task<User> GetUserAsync();
        Task<UserDish?> GetUserDishAsync(int userDishId);
        Task<List<UserDish>?> GetUserDishesAsync();
        Task<List<UserMealEntryDto>?> GetUserMealsForDate(DateTime date);
        Task<List<UserMealEntryDto>?> GetUserMealsForPeriod(DateTime start, DateTime end);
        Task<UserProduct?> GetUserProductAsync(int userProductId);
        Task<List<UserProduct>?> GetUserProductsAsync();
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task Logout();
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<bool> RemoveUserDishIngredientAsync(int dishId, int ingredientId);
        Task<bool> UpdateUserMealEntry(int userMealEntryId, UserMealEntryDto request);
    }
}