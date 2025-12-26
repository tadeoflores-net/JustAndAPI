using Microsoft.AspNetCore.Mvc;
using JustAndAPI.Services.OperationsApi.Services;


namespace JustAndAPI.Controllers
{
    namespace OperationsApi.Controllers
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
            public async Task<IActionResult> CreateOperation([FromBody] OperationRequest request)
            {
                _logger.LogInformation("Received operation request");

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

                try
                {
                    //Simular operación crítica
                    await _paymentService.ProcessPaymentAsync(
                        request
                    );

                    return Ok(new
                    {
                        operationId = request.OperationId,
                        status = "SUCCESS"
                    });
                }
                catch (Exception)
                {
                    return StatusCode(500, new
                    {
                        operationId = request.OperationId,
                        status = "FAILED",
                        message = "The operation could not be completed at this time"
                    });
                }
            }
        }
    }

}
