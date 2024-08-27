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

            try
            {
                var scraper = scraperFactory(website.ToLower());
                if (scraper == null)
                {
                    this.log.LogError(message: $"Website Scrape POST function Invalid website value: {website}");
                    return new NotFoundObjectResult(new { Error = $"Invalid website value: {website}" }) { StatusCode = 404 };
                }

                var search = request.Query;
                var location = request.Location;
                var dayRange = request.LastNDays;
                if (dayRange != null)
                {
                    if (dayRange < 0)
                    {
                        this.log.LogError(message: $"Website Scrape POST function Invalid lastNDays value: {dayRange}");
                        return new BadRequestObjectResult(new { Error = $"Invalid lastndays value: {dayRange}" }) { StatusCode = 403 }; ;  // Returns a 404 Not Found response
                    }
                }

                try
                {
                    var jobDataList = await scraper.ScrapeJobs(search, location, dayRange);
                    return Ok(jobDataList);
                }
                catch (Exception ex)
                {
                    this.log.LogError(message: "Unexpected error on Website Scrape POST .", exception: ex);
                    return new ObjectResult(ex.Message) { StatusCode = 500 };
                }
            }
            catch (Exception ex)
            {
                this.log.LogError(message: "Unexpected error on Website Scrape POST .", exception: ex);
                return new ObjectResult(ex.Message) { StatusCode = 500 };
            }            
        }
    }
}
