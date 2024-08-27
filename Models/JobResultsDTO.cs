namespace WebsiteScraper.Models
{
    public class MetaData
    {
        public Guid Query_Id { get; set; }
    }
    public class JobResultsDTO
    {
        public MetaData Meta { get; set; }
        public List<JobData> Results { get; set; }
    }
}
