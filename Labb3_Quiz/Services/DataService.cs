
using Labb3_Quiz.Models;
using System.Text.Json;
using System.IO;
using System.Windows;


namespace Labb3_Quiz.Services
{
    public class DataService
    {
        private readonly string _folderPath;
        private readonly string _filePath;

        public DataService()
        {
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Labb3_Quiz");

            Directory.CreateDirectory(_folderPath);

            _filePath = Path.Combine(_folderPath, "packs.json");
        }

        private static JsonSerializerOptions JsonOptions => new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true
        };

        public async Task<List<QuestionPack>> LoadPacksAsync()
        {
            if (!File.Exists(_filePath))
                return new List<QuestionPack>();

            try
            {
                var json = await File.ReadAllTextAsync(_filePath);
                return JsonSerializer.Deserialize<List<QuestionPack>>(json, JsonOptions) ?? new List<QuestionPack>();
            }
            catch (Exception ex) 
            {
                MessageBox.Show($"Couldn't read file {ex.Message}");
                return new List<QuestionPack>();
            }
        }

        public async Task SavePacksAsync(List<QuestionPack> packs)
        {
            var json = JsonSerializer.Serialize(packs, JsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
