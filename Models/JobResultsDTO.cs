using System.Text.Json.Serialization;

namespace WebsiteScraper.Models
{
    public class MetaData
    {
        [JsonPropertyName("query-id")]
        public int Query_Id { get; set; }
    }
    public class JobResultsDTO
    {
        public MetaData Meta { get; set; }
        public List<JobData> Results { get; set; }
    }
}
