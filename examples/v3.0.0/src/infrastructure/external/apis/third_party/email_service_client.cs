using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.External.Apis.ThirdParty
{
    public interface IEmailServiceClient
    {
        Task<EmailResponse> SendEmailAsync(EmailRequest request);
        Task<EmailStatusResponse> GetEmailStatusAsync(string emailId);
        Task<bool> ValidateEmailAsync(string emailAddress);
    }

    public class EmailServiceClient : IEmailServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly EmailServiceConfig _config;

        public EmailServiceClient(HttpClient httpClient, EmailServiceConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<EmailResponse> SendEmailAsync(EmailRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);
                _httpClient.DefaultRequestHeaders.Add("X-Client-Id", _config.ClientId);

                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/emails", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var emailResponse = JsonSerializer.Deserialize<EmailResponse>(responseContent);
                    return emailResponse;
                }
                else
                {
                    throw new EmailServiceException($"Email sending failed: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new EmailServiceException("Failed to send email", ex);
            }
        }

        public async Task<EmailStatusResponse> GetEmailStatusAsync(string emailId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);

                var response = await _httpClient.GetAsync($"{_config.BaseUrl}/emails/{emailId}/status");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var statusResponse = JsonSerializer.Deserialize<EmailStatusResponse>(responseContent);
                    return statusResponse;
                }
                else
                {
                    throw new EmailServiceException($"Failed to get email status: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new EmailServiceException("Failed to get email status", ex);
            }
        }

        public async Task<bool> ValidateEmailAsync(string emailAddress)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("X-API-Key", _config.ApiKey);

                var response = await _httpClient.GetAsync($"{_config.BaseUrl}/emails/validate?address={Uri.EscapeDataString(emailAddress)}");
                
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                throw new EmailServiceException("Failed to validate email", ex);
            }
        }
    }

    public class EmailServiceException : Exception
    {
        public EmailServiceException(string message) : base(message) { }
        public EmailServiceException(string message, Exception innerException) : base(message, innerException) { }
    }
}
