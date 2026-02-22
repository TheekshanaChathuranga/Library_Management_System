using BorrowingReturnsService.Dtos;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Services
{
    public class CatalogClient : ICatalogClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CatalogClient> _logger;
        private readonly string _username;
        private readonly string _password;

        public CatalogClient(HttpClient httpClient, IConfiguration configuration, ILogger<CatalogClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _username = configuration["CatalogService:Username"] ?? "admin";
            _password = configuration["CatalogService:Password"] ?? "admin123";
            
            // Set Basic Authentication header
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }

        public async Task<CatalogBookDto> GetBookByIdAsync(Guid bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Books/{bookId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get book {bookId} from CatalogService. Status: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var book = JsonSerializer.Deserialize<CatalogBookDto>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return book;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling CatalogService to get book {bookId}");
                throw;
            }
        }

        public async Task<bool> UpdateBookAvailabilityAsync(Guid bookId, bool isAvailable)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(isAvailable), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PatchAsync($"api/Books/{bookId}/availability", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to update book {bookId} availability in CatalogService. Status: {response.StatusCode}");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling CatalogService to update book {bookId} availability");
                return false;
            }
        }
    }
}
