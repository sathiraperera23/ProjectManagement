using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TaskManagementApi.Application.DTOs.Notifications;
using TaskManagementApi.Application.Interfaces;

namespace TaskManagementApi.Infrastructure.Services
{
    public class SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmsService> _logger;

        public SmsService(HttpClient httpClient, IConfiguration configuration, ILogger<SmsService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendSmsAsync(string mobileNumber, string message)
        {
            if (!bool.Parse(_configuration["Sms:Enabled"] ?? "false"))
            {
                _logger.LogInformation("SMS sending is disabled. Target: {Mobile}, Message: {Msg}", mobileNumber, message);
                return false;
            }

            try
            {
                var baseUrl = _configuration["Sms:CommonModuleUrl"];
                var endpoint = _configuration["Sms:Endpoint"];
                var request = new SmsRequest { Mobile = mobileNumber, Message = message };

                var response = await _httpClient.PostAsJsonAsync($"{baseUrl}{endpoint}", request);
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("SMS common module returned error: {Error}", error);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call SMS common module for {Mobile}", mobileNumber);
                return false;
            }
        }
    }
}
