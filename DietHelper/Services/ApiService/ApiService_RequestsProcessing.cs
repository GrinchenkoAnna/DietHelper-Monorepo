using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DietHelper.Services
{
    public partial class ApiService
    {
        /// <summary> 
        /// isSafeMethod == true - data remains unchanged (GET) <br/>
        /// isSafeMethod == false - data changes (POST, PUT, DELETE)
        /// </summary>
        private async Task<HttpResponseMessage?> SendRequestAsync(Func<Task<HttpResponseMessage>> sendRequest, bool isSafeMethod = true)
        {
            var response = await sendRequest();
            
            // 403
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                if (isSafeMethod) return null;
                throw new HttpRequestException("[SendRequestAsync]: 403");
            }
            
            // 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (await RefreshTokensAsync())
                {
                    response = await sendRequest();

                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        if (isSafeMethod) return null;
                        throw new HttpRequestException("[SendRequestAsync]: 401");
                    }

                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        if (isSafeMethod) return null;
                        throw new HttpRequestException("[SendRequestAsync]: 403 after 401");
                    }
                }
                else
                {
                    if (isSafeMethod) return null;
                    throw new HttpRequestException("[SendRequestAsync]: не удалось обновить токен");
                }
            }

            // 404, 500, ...
            if (!response.IsSuccessStatusCode)
            {
                if (isSafeMethod) return null;

                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"[SendRequestAsync]: {error}");
            }

            return response;
        }
    }
}
