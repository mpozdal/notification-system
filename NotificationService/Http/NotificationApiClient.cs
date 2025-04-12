using System.Net.Http.Json;
using NotificationShared.Models;
using System.Threading.Tasks;
namespace NotificationService.Http;

public class NotificationApiClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<NotificationApiClient> _logger;

    public NotificationApiClient(IHttpClientFactory httpClientFactory, ILogger<NotificationApiClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<Notification?> GetNotificationDetailsAsync(Guid notificationId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("NotificationAPI");
            var response = await client.GetAsync($"http://localhost:5008/api/Notification/{notificationId}");
            _logger.LogInformation(response.Content.ReadAsStringAsync().Result);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get notification {NotificationId}. StatusCode: {StatusCode}", notificationId, response.StatusCode);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<Notification>();
            return dto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while fetching notification {NotificationId}", notificationId);
            return null;
        }
    }

}