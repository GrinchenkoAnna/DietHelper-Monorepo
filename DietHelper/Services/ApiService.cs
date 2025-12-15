using DietHelper.Common.Models.Products;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        public ApiService() 
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("http://localhost:5119/api/")
            };
        }

        public async Task<UserProduct?> GetUserProductMockAsync()
        {
            var response = await _httpClient.GetAsync("simpleproducts/mock");

            if (response.StatusCode == HttpStatusCode.NotFound) return null;

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<UserProduct>();
        }
    }
}
