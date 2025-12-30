namespace JustAndAPI.Models
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? ErrorType { get; set; }
        public string? Message { get; set; }
    }
}
