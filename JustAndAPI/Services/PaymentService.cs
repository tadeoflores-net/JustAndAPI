using System.Net;
using System.Text.Json;
using JustAndAPI.Controllers;
using JustAndAPI.Models;
using JustAndAPI.Support;

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

        public async Task<PaymentResult> ProcessPaymentAsync(
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

                if (response.StatusCode == HttpStatusCode.FailedDependency)
                {
                    _logger.LogError(
                        "Legacy provider failed for operation {OperationId}",
                        request.OperationId);

                    AddToDeadLetterQueue(
                        request,
                        "LEGACY_ERROR - FailedDependency");

                    return new PaymentResult
                    {
                        Success = false,
                        ErrorType = "LEGACY_ERROR",
                        Message = "External payment provider failed, try again later."
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Legacy provider unavailable for operation {OperationId}",
                        request.OperationId);

                    AddToDeadLetterQueue(
                        request,
                        $"LEGACY_UNAVAILABLE - StatusCode {(int)response.StatusCode}");

                    return new PaymentResult
                    {
                        Success = false,
                        ErrorType = "LEGACY_UNAVAILABLE",
                        Message = "External payment provider is not operating"
                    };
                }

                var providerResponse =
                    await response.Content.ReadFromJsonAsync<OperationResponse>();

                _logger.LogInformation(
                    "Payment approved for operation {OperationId}",
                    request.OperationId);

                return new PaymentResult
                {
                    Success = true
                };
            }
            catch (TaskCanceledException)
            {
                _logger.LogError(
                    "Timeout calling Java payment provider for operation {OperationId}",
                    request.OperationId);

                AddToDeadLetterQueue(
                    request,
                    "TIMEOUT - Java provider timeout");

                return new PaymentResult
                {
                    Success = false,
                    ErrorType = "TIMEOUT",
                    Message = "Sorry, server didn't response, timeout"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error processing payment for operation {OperationId}",
                    request.OperationId);

                AddToDeadLetterQueue(
                    request,
                    $"INTERNAL_ERROR - {ex.Message}");

                return new PaymentResult
                {
                    Success = false,
                    ErrorType = "INTERNAL_ERROR",
                    Message = "Unexpected error, server may be down."
                };
            }
        }

        private void AddToDeadLetterQueue(
            OperationRequest request,
            string error)
        {
            DeadLetterQueue.Add(new DeadLetterItem
            {
                OperationId = request.OperationId,
                Payload = JsonSerializer.Serialize(request),
                Error = error
            });
        }
    }
}
