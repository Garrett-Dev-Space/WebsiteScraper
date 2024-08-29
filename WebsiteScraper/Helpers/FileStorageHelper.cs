using System.Text.Json;
using WebsiteScraper.Models;

namespace WebsiteScraper.Helpers
{
    public class FileStorageHelper
    {
        private static readonly string ResultsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "results");
        private static readonly string ResultsFilePath = Path.Combine(ResultsFolderPath, "results.json");

        public static List<JobResults> LoadResults()
        {
            if (File.Exists(ResultsFilePath))
            {
                var json = File.ReadAllText(ResultsFilePath);
                var scrapeData = JsonSerializer.Deserialize<List<JobResults>>(json);
                return scrapeData;
            }
            return (new List<JobResults>());
        }

        public static void SaveResults(List<JobResults> results)
        {
            // Create the directory if needed
            if (!Directory.Exists(ResultsFolderPath))
            {
                Directory.CreateDirectory(ResultsFolderPath);
            }

            var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ResultsFilePath, json);
        }
    }
}
