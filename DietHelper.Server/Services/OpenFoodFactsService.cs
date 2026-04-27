using DietHelper.Common.DTO;
using DietHelper.Server.Models;
using System.Diagnostics;
using System.Text.Json;

namespace DietHelper.Server.Services
{
    public interface IOpenFoodFactsService
    {
        Task<OpenFoodFactsDto> GetProductByBarcodeAsync(string barcode);
    }

    public class OpenFoodFactsService : IOpenFoodFactsService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://world.openfoodfacts.org/api/v2/";

        public OpenFoodFactsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("DietHelper/1.0 (ann@grin4enko.com)");
        }

        public async Task<OpenFoodFactsDto?> GetProductByBarcodeAsync(string barcode)
        {
            try
            {
                var response = await _httpClient.GetAsync($"product/{barcode}.json");

                if (!response.IsSuccessStatusCode)
                {
                    return new OpenFoodFactsDto()
                    {
                        Barcode = barcode,
                        Message = $"Возвращен статус {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Response from API: {json}");

                var product = JsonSerializer.Deserialize<OFFProduct>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (product?.Status != 1 || product is null)
                    return new OpenFoodFactsDto()
                    {
                        Barcode = barcode,
                        Message = "Продукт не найден"
                    };

                var productInfo = product.ProductInfo;

                return new OpenFoodFactsDto()
                {
                    Barcode = barcode,
                    Name = productInfo.Name + (string.IsNullOrEmpty(productInfo.Brand) ? "" : $" - {productInfo.Brand}"),
                    Calories = productInfo.Nutriments.Calories,
                    Protein = productInfo.Nutriments.Protein,
                    Fat = productInfo.Nutriments.Fat,
                    Carbs = productInfo.Nutriments.Carbs
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }
    }
}
