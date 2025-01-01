using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace MapIt.Services
{
    public class JsonKeyMappingService : IKeyMappingService
    {
        public JsonKeyMappingService(string filePath = "keyMappings.json")
        {
            FilePath = filePath;
        }

        public string FilePath { get; }

        public Dictionary<Keys, ushort> LoadKeyMappings()
        {
            var keyMapping = new Dictionary<Keys, ushort>();

            if (!File.Exists(FilePath))
            {
                Console.WriteLine($"The key mappings JSON file '{FilePath}' was not found.");
                return keyMapping;  // Return an empty collection if the file doesn't exist
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                keyMapping = JsonSerializer.Deserialize<Dictionary<Keys, ushort>>(json);

                if (keyMapping == null)
                {
                    Console.WriteLine("The JSON file is empty or malformed.");
                    return new Dictionary<Keys, ushort>(); // Return an empty collection in case of deserialization issues
                }

                Console.WriteLine("Key mappings successfully loaded from JSON.");
            }
            catch (JsonException jsonEx)
            {
                Console.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                MessageBox.Show($"JSON deserialization error: {jsonEx.Message}", "Configuration Error");
            }
            catch (IOException ioEx)
            {
                Console.WriteLine($"File access error: {ioEx.Message}");
                MessageBox.Show($"File access error: {ioEx.Message}", "Configuration Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error: {ex.Message}");
                MessageBox.Show($"Unknown error: {ex.Message}", "Configuration Error");
            }

            return keyMapping;
        }
    }

}
