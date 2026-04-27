using DietHelper.Common.DTO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        public async Task<OpenFoodFactsDto?> GetProductFromOpenFoodFactsAsync(string barcode)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"openfoodfacts/barcode/{barcode}"), true);
                if (response is null) return null;

                return await response.Content.ReadFromJsonAsync<OpenFoodFactsDto?>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }
    }
}
