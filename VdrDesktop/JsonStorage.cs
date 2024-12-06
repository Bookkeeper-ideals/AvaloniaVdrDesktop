using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using VdrDesktop.Models;

namespace VdrDesktop
{
    public class JsonStorage
    {
        private readonly string _filePath;

        public JsonStorage(string filePath)
        {
            _filePath = filePath;
        }

        // Read configuration from JSON file
        public SyncSettings? LoadConfig()
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<SyncSettings>(json);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading config: {ex.Message}");
                }
            }
            return null;
        }

        // Write configuration to JSON file
        public async Task SaveConfigAsync(SyncSettings settings)
        {
            try
            {
                string json = JsonSerializer.Serialize(settings);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing config: {ex.Message}");
            }
        }
    }
}
