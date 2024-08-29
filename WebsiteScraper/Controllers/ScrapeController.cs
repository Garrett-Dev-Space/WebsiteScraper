using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using System.Xml.Linq;
using WebsiteScraper.Models;
using WebsiteScraper.Services;
using WebsiteScraper.Helpers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebsiteScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapeController : ControllerBase
    {
        private readonly ILogger<ScrapeController> log;
        private readonly Func<string, IWebScraper> scraperFactory;
        private List<JobResults> queryHistory;
        private static readonly string ResultsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Results");
        private static readonly string ResultsFilePath = Path.Combine(ResultsFolderPath, "scraper_results.json");

        public ScrapeController(ILogger<ScrapeController> log, Func<string, IWebScraper> scraperFactory)
        {
            this.log = log;
            this.scraperFactory = scraperFactory;
            var data = FileStorageHelper.LoadResults();
            queryHistory = data;
        }

        // GET api/<ScrapeController>/5
        [HttpPost("{website}")]
        public async Task<ActionResult<JobResults>> PostAsync(string website, [FromBody] ScrapeRequest request)
        {
            this.log.LogInformation("Website Scrape POST function processed a request.");

            var scraper = scraperFactory(website.ToLower());
            if (scraper == null)
            {
                this.log.LogError(message: $"Website Scrape POST function Invalid website value: {website}.");
                return NotFound(new { Error = $"Invalid website value: {website}" });
            }

            // Grab the user input from the POST body
            var search = request.Query;
            var location = request.Location;
            var dayRange = request.LastNDays;
            if (!ValidateDayRange(dayRange))
            {
                return BadRequest(new { Error = $"Invalid lastndays value: {dayRange}. Must be a positive integer, greater than 0." });
            }

            try
            {
                var jobDataList = await scraper.ScrapeJobs(search, location, dayRange);
                var results = new JobResults()
                {
                    Meta = new MetaData()   { Query_Id = Guid.NewGuid() },
                    Results = jobDataList
                };

                // Add the results to the local storage containing all of the results
                queryHistory.Add(results);

                // Save the new results list
                FileStorageHelper.SaveResults(queryHistory);

                // Return the results from this query
                return Ok(results);
            }
            catch (Exception ex)
            {
                this.log.LogError(message: "Unexpected error on Website Scrape POST .", exception: ex);
                return Forbid($"{website} is temporarily blocking the scraper.");
            }          
        }

        // GET api/<ScrapeController>/5
        [HttpGet("{query-id}")]
        public ActionResult<JobResults> Get([FromRoute(Name = "query-id")] Guid query_id)
        {
            this.log.LogInformation("Website Scrape GET function processed a request.");

            var searchResult = queryHistory.FirstOrDefault(scrapeData => scrapeData.Meta.Query_Id == query_id);
            if (searchResult != null)
            {
                return Ok(searchResult);
            }
            else
            {
                return NotFound(new { Error = $"Result with query-id {query_id} could not be found." });
            }
        }

        private bool ValidateDayRange(int? dayRange)
        {
            if (dayRange.HasValue && dayRange < 0)
            {
                this.log.LogError($"Invalid lastNDays value: {dayRange}.");
                return false;
            }
            return true;
        }
    }
}
