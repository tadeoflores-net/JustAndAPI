using JustAndAPI.Models;
using System.Text.Json;

namespace JustAndAPI.Support
{
    public static class DeadLetterQueue
    {
        private static readonly List<DeadLetterItem> _items = new();
        private static readonly string _filePath = "dead-letter-queue.json";

        public static void Add(DeadLetterItem item)
        {
            _items.Add(item);

            var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }

        public static IReadOnlyList<DeadLetterItem> GetAll()
        {
            return _items.AsReadOnly();
        }
    }
}
