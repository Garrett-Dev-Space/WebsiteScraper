using System.ComponentModel;

namespace WebsiteScraper.Models
{
    public class ScrapeRequest
    {
        [DefaultValue("Dispensary")]
        public string? Query { get; set; }
        [DefaultValue("California")]
        public string? Location { get; set; }
        [DefaultValue(7)]
        public int? LastNDays { get; set; }
    }
}
