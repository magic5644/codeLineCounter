using System.Text.Json;

namespace CodeLineCounter.Utils
{
    public static class JsonHandler
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static void Serialize<T>(IEnumerable<T> data, string filePath)
        {
            string jsonString = JsonSerializer.Serialize(data, _options);
            File.WriteAllText(filePath, jsonString);
        }

        public static IEnumerable<T> Deserialize<T>(string filePath)
        {
            string jsonString = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<IEnumerable<T>>(jsonString, _options) 
                   ?? [];
        }
    }
}