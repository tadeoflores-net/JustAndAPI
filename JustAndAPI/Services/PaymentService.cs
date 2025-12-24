namespace JustAndAPI.Services
{
    using Microsoft.Extensions.Logging;

    namespace OperationsApi.Services
    {
        public class PaymentService
        {
            private readonly ILogger<PaymentService> _logger;
            private readonly Random _random = new Random();

            public PaymentService(ILogger<PaymentService> logger)
            {
                _logger = logger;
            }

            public async Task ProcessPaymentAsync(string operationId, decimal amount)
            {
                try
                {
                    // Simular latencia externa (0.5s – 3s)
                    int delay = _random.Next(500, 3000);
                    await Task.Delay(delay);

                    // Simular fallo externo (40% de probabilidad)
                    if (_random.Next(1, 100) <= 40)
                    {
                        throw new Exception("External payment provider failed");
                    }

                    _logger.LogInformation(
                        "Payment processed successfully for operation {OperationId} with amount {Amount}",
                        operationId,
                        amount
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Error while processing payment for operation {OperationId}",
                        operationId
                    );
                    throw;
                }
            }
        }
    }
}
