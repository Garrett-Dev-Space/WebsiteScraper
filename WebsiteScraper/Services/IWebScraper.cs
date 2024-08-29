using WebsiteScraper.Models;

namespace WebsiteScraper.Services
{
    public interface IWebScraper
    {
        Task<List<JobData>> ScrapeJobs(string search, string location, int? dayRange);
    }
}
