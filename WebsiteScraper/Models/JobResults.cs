using System.Text.Json.Serialization;

namespace WebsiteScraper.Models
{
    public class MetaData
    {
        [JsonPropertyName("query-id")]
        public Guid Query_Id { get; set; }
    }
    public class JobResults
    {
        public MetaData Meta { get; set; }
        public List<JobData> Results { get; set; }
    }
}