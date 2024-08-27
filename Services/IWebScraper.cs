using WebsiteScraper.Models;

namespace WebsiteScraper.Services
{
    public interface IWebScraper
    {
        Task<JobResultsDTO> ScrapeJobs(string search, string location, int? dayRange);
    }
}
