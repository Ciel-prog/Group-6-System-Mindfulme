using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Mime;
using System.Net.Http.Headers;
using MindfulMe.Models;

namespace MindfulMe.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://localhost:7073/api/";

        public ApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true // For development only
            };

            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri(BaseUrl)
            };

            // Prefer JSON responses
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        // ========== AUTH ENDPOINTS ==========

        public async Task<(bool Success, string? Message, User? User)> LoginAsync(string username, string password)
        {
            try
            {
                var endpoint = "auth/login";
                var response = await _httpClient.PostAsJsonAsync(endpoint, new { username, password });
                var contentString = await response.Content.ReadAsStringAsync();

                // If not success, return the raw server message for diagnostics
                if (!response.IsSuccessStatusCode)
                {
                    var preview = string.IsNullOrWhiteSpace(contentString) ? "(empty body)" : contentString;
                    return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Body: {preview}", null);
                }

                // Ensure server sent JSON before deserializing
                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.Equals(mediaType, MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"Unexpected content type '{mediaType}'. Body: {contentString}", null);
                }

                var result = JsonSerializer.Deserialize<LoginResponse>(contentString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Success == true && result.User != null)
                {
                    var user = new User
                    {
                        Id = result.User.Id,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        CreatedAt = result.User.CreatedAt
                    };
                    return (true, result.Message, user);
                }

                return (false, result?.Message ?? "Login failed", null);
            }
            catch (HttpRequestException ex)
            {
                var detailedMessage = $"Cannot connect to API at {BaseUrl}\nError: {ex.Message}\n" +
                                      $"Make sure the MindfulMe.API project is running on https://localhost:7073";
                return (false, detailedMessage, null);
            }
            catch (JsonException ex)
            {
                return (false, $"Invalid JSON from server: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}", null);
            }
        }

        public async Task<(bool Success, string? Message, User? User)> RegisterAsync(string username, string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/register", new { username, email, password });
                var contentString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    var preview = string.IsNullOrWhiteSpace(contentString) ? "(empty body)" : contentString;
                    return (false, $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. Body: {preview}", null);
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType;
                if (!string.Equals(mediaType, MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(mediaType, "application/problem+json", StringComparison.OrdinalIgnoreCase))
                {
                    return (false, $"Unexpected content type '{mediaType}'. Body: {contentString}", null);
                }

                var result = JsonSerializer.Deserialize<LoginResponse>(contentString, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result?.Success == true && result.User != null)
                {
                    var user = new User
                    {
                        Id = result.User.Id,
                        Username = result.User.Username,
                        Email = result.User.Email,
                        CreatedAt = result.User.CreatedAt
                    };
                    return (true, result.Message, user);
                }

                return (false, result?.Message ?? "Registration failed", null);
            }
            catch (HttpRequestException ex)
            {
                return (false, $"Connection error: {ex.Message}. Make sure the API is running on https://localhost:7073", null);
            }
            catch (JsonException ex)
            {
                return (false, $"Invalid JSON from server: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                return (false, $"Connection error: {ex.Message}", null);
            }
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<User>($"auth/user/{id}");
            }
            catch
            {
                return null;
            }
        }

        // ========== JOURNAL ENDPOINTS ==========

        public async Task<List<JournalEntry>?> GetJournalEntriesAsync(int userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<JournalEntry>>($"journal/user/{userId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<JournalEntry?> GetJournalEntryByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<JournalEntry>($"journal/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<JournalEntry?> CreateJournalEntryAsync(int userId, string content, int moodScore, int stressLevel, double hoursSlept)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("journal", new
                {
                    userId,
                    content,
                    moodScore,
                    stressLevel,
                    hoursSlept
                });
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<JournalEntry>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? Error)> UpdateJournalEntryAsync(JournalEntry entry)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"journal/{entry.Id}", entry);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> DeleteJournalEntryAsync(int id, int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"journal/{id}?userId={userId}");

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ========== MEDITATION ENDPOINTS ==========

        public async Task<List<MeditationSession>?> GetMeditationSessionsAsync(int userId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<List<MeditationSession>>($"meditation/user/{userId}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<MeditationSession?> GetMeditationSessionByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<MeditationSession>($"meditation/{id}");
            }
            catch
            {
                return null;
            }
        }

        public async Task<MeditationSession?> CreateMeditationSessionAsync(int userId, int durationMinutes, string meditationType, int qualityRating)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("meditation", new
                {
                    userId,
                    durationMinutes,
                    meditationType,
                    qualityRating
                });
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<MeditationSession>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? Error)> UpdateMeditationSessionAsync(MeditationSession session)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"meditation/{session.Id}", session);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? Error)> DeleteMeditationSessionAsync(int id, int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"meditation/{id}?userId={userId}");

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // ========== HELPER CLASSES ==========

        private class LoginResponse
        {
            public bool Success { get; set; }
            public string? Message { get; set; }
            public UserDto? User { get; set; }
        }

        private class UserDto
        {
            public int Id { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }
    }
}