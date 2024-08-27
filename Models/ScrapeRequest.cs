namespace WebsiteScraper.Models
{
    public class ScrapeRequest
    {
        public string? Query { get; set; }
        public string? Location { get; set; }
        public int? LastNDays { get; set; }
    }
}
