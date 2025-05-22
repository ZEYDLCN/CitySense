using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using CitySense.Shared.Dtos; 

namespace CitySense.Frontend.Services
{
    public class SensorDataService
    {
        private readonly HttpClient _httpClient;

        public SensorDataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<SensorDataPointDto>> GetLatestSensorDataAsync(int count = 10)
        {
            try
            {
                var requestUri = $"api/sensordata/latest?count={count}";
                var result = await _httpClient.GetFromJsonAsync<List<SensorDataPointDto>>(requestUri);
                return result ?? new List<SensorDataPointDto>();
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"API isteği hatası: {ex.Message}");
                return new List<SensorDataPointDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmedik hata: {ex.Message}");
                return new List<SensorDataPointDto>();
            }
        }
    }
}
