using Microsoft.AspNetCore.Mvc;
using JustAndAPI.Services.OperationsApi.Services;
using JustAndAPI.Models;

namespace JustAndAPI.Controllers.OperationsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OperationsController : ControllerBase
    {
        private readonly ILogger<OperationsController> _logger;
        private readonly PaymentService _paymentService;

        public OperationsController(
            ILogger<OperationsController> logger,
            PaymentService paymentService)
        {
            _logger = logger;
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOperation(
            [FromBody] OperationRequest request)
        {
            _logger.LogInformation("Received operation request");

            // 1️⃣ Validación
            if (request == null ||
                string.IsNullOrWhiteSpace(request.OperationId) ||
                string.IsNullOrWhiteSpace(request.CustomerId) ||
                request.Amount <= 0)
            {
                _logger.LogWarning("Invalid operation request received");

                return BadRequest(new
                {
                    error = "Invalid request data"
                });
            }

            // 2️⃣ Ejecutar operación crítica
            PaymentResult result =
                await _paymentService.ProcessPaymentAsync(request);

            // 3️⃣ Manejo explícito de errores
            if (!result.Success)
            {
                return result.ErrorType switch
                {
                    "TIMEOUT" => StatusCode(504, new
                    {
                        customerId = request.CustomerId,
                        operationId = request.OperationId,
                        status = "FAILED",
                        errorType = result.ErrorType,
                        message = result.Message
                    }),

                    "LEGACY_ERROR" => StatusCode(424, new
                    {
                        customerId = request.CustomerId,
                        operationId = request.OperationId,
                        status = "FAILED",
                        errorType = result.ErrorType,
                        message = result.Message
                    }),

                    "LEGACY_UNAVAILABLE" => StatusCode(503, new
                    {
                        customerId = request.CustomerId,
                        operationId = request.OperationId,
                        status = "FAILED",
                        errorType = result.ErrorType,
                        message = result.Message
                    }),

                    "PAYMENT_REJECTED" => BadRequest(new
                    {
                        customerId = request.CustomerId,
                        operationId = request.OperationId,
                        status = "FAILED",
                        errorType = result.ErrorType,
                        message = result.Message
                    }),

                    _ => StatusCode(500, new
                    {
                        customerId = request.CustomerId,
                        operationId = request.OperationId,
                        status = "FAILED",
                        errorType = result.ErrorType,
                        message = result.Message
                    })
                };
            }

            // 4️⃣ Éxito
            return Ok(new
            {
                operationId = request.OperationId,
                status = "SUCCESS"
            });
        }
    }
}
