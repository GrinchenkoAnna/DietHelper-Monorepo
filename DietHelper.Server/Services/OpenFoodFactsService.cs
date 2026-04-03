using DietHelper.Common.DTO;
using System.Diagnostics;

namespace DietHelper.Server.Services
{
    public interface IOpenFoodFactsService
    {
        Task<OpenFoodFactsDto> GetProductByBarcodeAsync(string barcode);
    }

    public class OpenFoodFactsService : IOpenFoodFactsService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://world.openfoodfacts.org/api/v2";

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
                var response = await _httpClient.GetAsync($"products/{barcode}.json");

                if (!response.IsSuccessStatusCode)
                {
                    return new OpenFoodFactsDto()
                    {
                        Barcode = barcode,
                        Message = $"Возвращен статус {response.StatusCode}"
                    };
                }

                var json = await response.Content.ReadAsStringAsync();
                //дописать


                return new OpenFoodFactsDto()
                {
                    //дописать
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
