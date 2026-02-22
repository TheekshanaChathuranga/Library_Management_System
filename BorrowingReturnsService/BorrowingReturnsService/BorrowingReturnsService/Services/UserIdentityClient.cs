using BorrowingReturnsService.Dtos.UserIdentity;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BorrowingReturnsService.Services
{
    public class UserIdentityClient : IUserIdentityClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserIdentityClient> _logger;
        
        private string? _cachedAccessToken;
        private DateTime _tokenExpiresAt = DateTime.MinValue;
        private readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

        public UserIdentityClient(
            HttpClient httpClient, 
            IConfiguration configuration,
            ILogger<UserIdentityClient> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<UserSummary?> GetUserAsync(Guid userId)
        {
            try
            {
                // Ensure we have a valid token
                var token = await EnsureTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to acquire access token for UserIdentityService");
                    return null;
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.GetAsync($"/api/Users/{userId}");
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning($"User {userId} not found in UserIdentityService");
                    return null;
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<UserSummary>(content, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                _logger.LogInformation($"Retrieved user {userId} from UserIdentityService: {user?.Email}");
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving user {userId} from UserIdentityService");
                return null;
            }
        }

        private async Task<string?> EnsureTokenAsync()
        {
            // Check if we have a valid cached token
            if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTime.UtcNow < _tokenExpiresAt)
            {
                return _cachedAccessToken;
            }

            // Acquire lock to prevent multiple token requests
            await _tokenLock.WaitAsync();
            try
            {
                // Double-check after acquiring lock
                if (!string.IsNullOrEmpty(_cachedAccessToken) && DateTime.UtcNow < _tokenExpiresAt)
                {
                    return _cachedAccessToken;
                }

                // Acquire new token
                var email = _configuration["UserIdentity:ServiceAccount:Email"];
                var password = _configuration["UserIdentity:ServiceAccount:Password"];

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    _logger.LogError("UserIdentity service account credentials not configured");
                    return null;
                }

                var loginRequest = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var jsonContent = JsonSerializer.Serialize(loginRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/auth/login", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Failed to login to UserIdentityService: {response.StatusCode} - {errorContent}");
                    return null;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseContent, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (authResponse == null || string.IsNullOrEmpty(authResponse.AccessToken))
                {
                    _logger.LogError("Received invalid auth response from UserIdentityService");
                    return null;
                }

                // Cache the token
                _cachedAccessToken = authResponse.AccessToken;
                _tokenExpiresAt = authResponse.ExpiresAt.AddSeconds(-60); // Refresh 1 minute before expiry
                
                _logger.LogInformation($"Successfully acquired access token from UserIdentityService, expires at {_tokenExpiresAt}");
                
                return _cachedAccessToken;
            }
            finally
            {
                _tokenLock.Release();
            }
        }
    }
}
