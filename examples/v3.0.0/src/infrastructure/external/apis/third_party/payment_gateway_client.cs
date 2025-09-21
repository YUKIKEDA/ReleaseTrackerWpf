using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Infrastructure.External.Apis.ThirdParty
{
    public interface IPaymentGatewayClient
    {
        Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request);
        Task<RefundResponse> ProcessRefundAsync(RefundRequest request);
        Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionId);
    }

    public class PaymentGatewayClient : IPaymentGatewayClient
    {
        private readonly HttpClient _httpClient;
        private readonly PaymentGatewayConfig _config;

        public PaymentGatewayClient(HttpClient httpClient, PaymentGatewayConfig config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");
                _httpClient.DefaultRequestHeaders.Add("X-Client-Version", _config.ClientVersion);

                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/payments", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(responseContent);
                    return paymentResponse;
                }
                else
                {
                    throw new PaymentGatewayException($"Payment failed: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new PaymentGatewayException("Failed to process payment", ex);
            }
        }

        public async Task<RefundResponse> ProcessRefundAsync(RefundRequest request)
        {
            try
            {
                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

                var response = await _httpClient.PostAsync($"{_config.BaseUrl}/refunds", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var refundResponse = JsonSerializer.Deserialize<RefundResponse>(responseContent);
                    return refundResponse;
                }
                else
                {
                    throw new PaymentGatewayException($"Refund failed: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new PaymentGatewayException("Failed to process refund", ex);
            }
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(string transactionId)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_config.ApiKey}");

                var response = await _httpClient.GetAsync($"{_config.BaseUrl}/payments/{transactionId}/status");
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var statusResponse = JsonSerializer.Deserialize<PaymentStatusResponse>(responseContent);
                    return statusResponse;
                }
                else
                {
                    throw new PaymentGatewayException($"Failed to get payment status: {response.StatusCode} - {responseContent}");
                }
            }
            catch (Exception ex)
            {
                throw new PaymentGatewayException("Failed to get payment status", ex);
            }
        }
    }

    public class PaymentGatewayException : Exception
    {
        public PaymentGatewayException(string message) : base(message) { }
        public PaymentGatewayException(string message, Exception innerException) : base(message, innerException) { }
    }
}
