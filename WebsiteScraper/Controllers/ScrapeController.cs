using HtmlAgilityPack;
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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebsiteScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapeController : ControllerBase
    {
        private readonly ILogger<ScrapeController> log;
        private readonly Func<string, IWebScraper> scraperFactory;
        private static readonly Dictionary<int, JobResultsDTO> queryHistory = new Dictionary<int, JobResultsDTO>();
        private static int queryCount = 1;

        public ScrapeController(ILogger<ScrapeController> log, Func<string, IWebScraper> scraperFactory)
        {
            this.log = log;
            this.scraperFactory = scraperFactory;
        }

        // GET api/<ScrapeController>/5
        [HttpPost("{website}")]
        public async Task<ActionResult<JobResultsDTO>> PostAsync(string website, [FromBody] ScrapeRequest request)
        {
            this.log.LogInformation("Website Scrape POST function processed a request.");

            var scraper = scraperFactory(website.ToLower());
            if (scraper == null)
            {
                this.log.LogError(message: $"Website Scrape POST function Invalid website value: {website}");
                return new NotFoundObjectResult(new { Error = $"Invalid website value: {website}" }) { StatusCode = 404 };
            }

            // Grab the user input from the POST body
            var search = request.Query;
            var location = request.Location;
            var dayRange = request.LastNDays;
            if (!ValidateDayRange(dayRange))
            {
                return new BadRequestObjectResult(new { Error = $"Invalid lastndays value: {dayRange}" }) { StatusCode = 403 };
            }

            try
            {
                var jobDataList = await scraper.ScrapeJobs(search, location, dayRange);
                var results = new JobResultsDTO()
                {
                    Meta = new MetaData()   { Query_Id = queryCount },
                    Results = jobDataList

                };
                queryHistory[results.Meta.Query_Id] = results;
                queryCount++;
                return Ok(results);
            }
            catch (Exception ex)
            {
                this.log.LogError(message: "Unexpected error on Website Scrape POST .", exception: ex);
                return new ObjectResult(ex.Message) { StatusCode = 500 };
            }          
        }

        // GET api/<ScrapeController>/5
        [HttpGet("{query-id}")]
        public ActionResult<JobResultsDTO> Get([FromRoute(Name = "query-id")] int query_id)
        {
            this.log.LogInformation("Website Scrape GET function processed a request.");
            if (queryHistory.TryGetValue(query_id, out var result))
            {
                return Ok(result);
            }
            else
            {
                return NotFound(new { Error = $"Query with id {query_id} could not be found." });
            }
        }

        private bool ValidateDayRange(int? dayRange)
        {
            if (dayRange.HasValue && dayRange < 0)
            {
                this.log.LogError($"Invalid lastNDays value: {dayRange}");
                return false;
            }
            return true;
        }
    }
}
