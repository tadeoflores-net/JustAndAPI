namespace JustAndAPI.Models
{
    public class DeadLetterItem
    {
        public string OperationId { get; set; }
        public string Payload { get; set; }
        public string Error { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
