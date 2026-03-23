using DietHelper.Common.DTO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        public async Task<List<UserMealEntryDto>?> GetUserMealsForPeriod(DateTime start, DateTime end)
        {
            if (!IsAuthenticated) LoadSavedSession();

            try
            {
                var response = await SendRequestAsync(() => _httpClient.GetAsync($"meals/period?start={start:yyyy-MM-dd}&end={end:yyyy-MM-dd}"));
                if (response is null) return null;

                return await response.Content.ReadFromJsonAsync<List<UserMealEntryDto>>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ApiService]: {ex.Message}");
                return null;
            }
        }
    }
}
