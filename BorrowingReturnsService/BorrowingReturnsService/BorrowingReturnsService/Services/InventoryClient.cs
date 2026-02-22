using BorrowingReturnsService.Dtos;
using BorrowingReturnsService.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Services
{
    public class InventoryClient : IInventoryClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<InventoryClient> _logger;

        public InventoryClient(HttpClient httpClient, ILogger<InventoryClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<InventorySummaryDto> GetInventoryAsync(Guid bookId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/Inventory/{bookId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Failed to get inventory for book {bookId}. Status: {response.StatusCode}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var inventory = JsonSerializer.Deserialize<InventorySummaryDto>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling InventoryService to get inventory for book {bookId}");
                throw;
            }
        }

        public async Task<InventorySummaryDto> BorrowAsync(Guid bookId, BorrowChannel channel, string reference)
        {
            try
            {
                var request = new AdjustInventoryDto
                {
                    Quantity = 1,
                    Channel = (int)channel,
                    Reference = reference
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync($"api/Inventory/{bookId}/borrow", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to borrow book {bookId} from InventoryService. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new InvalidOperationException($"Failed to adjust inventory: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var inventory = JsonSerializer.Deserialize<InventorySummaryDto>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling InventoryService to borrow book {bookId}");
                throw;
            }
        }

        public async Task<InventorySummaryDto> ReturnAsync(Guid bookId, BorrowChannel channel, string reference)
        {
            try
            {
                var request = new AdjustInventoryDto
                {
                    Quantity = 1,
                    Channel = (int)channel,
                    Reference = reference
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(request), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync($"api/Inventory/{bookId}/return", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Failed to return book {bookId} to InventoryService. Status: {response.StatusCode}, Error: {errorContent}");
                    throw new InvalidOperationException($"Failed to adjust inventory: {errorContent}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var inventory = JsonSerializer.Deserialize<InventorySummaryDto>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                return inventory;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calling InventoryService to return book {bookId}");
                throw;
            }
        }
    }
}
