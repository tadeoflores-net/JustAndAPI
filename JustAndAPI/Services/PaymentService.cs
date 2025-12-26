using JustAndAPI.Controllers;
using JustAndAPI.Models;


namespace JustAndAPI.Services.OperationsApi.Services
{

    public class PaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            HttpClient httpClient,
            ILogger<PaymentService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task ProcessPaymentAsync(
            OperationRequest request)
        {
            try
            {
                _logger.LogInformation(
                    "Calling Java payment provider for operation {OperationId}",
                    request.OperationId);

                var response = await _httpClient.PostAsJsonAsync(
                    "/legacy/validate",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Java payment provider returned status {StatusCode} for operation {OperationId}",
                        response.StatusCode,
                        request.OperationId);

                    throw new Exception("External payment provider failed");
                }

                var providerResponse =
                    await response.Content.ReadFromJsonAsync<OperationResponse>();

                if (providerResponse?.Status != "OK")
                {
                    _logger.LogWarning(
                        "Payment rejected for operation {OperationId}: {Message}",
                        request.OperationId,
                        providerResponse?.Message);

                    throw new Exception("Payment was rejected by provider");
                }

                _logger.LogInformation(
                    "Payment approved for operation {OperationId}",
                    request.OperationId);
            }
            catch (TaskCanceledException)
            {
                _logger.LogError(
                    "Timeout calling Java payment provider for operation {OperationId}",
                    request.OperationId);

                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while processing payment for operation {OperationId}",
                    request.OperationId);

                throw;
            }
        }
    }
}
