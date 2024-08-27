using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;
using WebsiteScraper.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebsiteScraper.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScrapeController : ControllerBase
    {
        private readonly ILogger<ScrapeController> log;
        private readonly HttpClient _httpClient;

        public ScrapeController(ILogger<ScrapeController> log, IHttpClientFactory httpClient)
        {
            this.log = log;
            this._httpClient = httpClient.CreateClient("ScraperClient");
        }

        // GET api/<ScrapeController>/5
        [HttpPost("{website}")]
        public ActionResult<string> Post(string website, [FromBody] ScrapeRequest request)
        {
            this.log.LogInformation("C# HTTP Scrape POST function processed a request.");

            try
            {
                if (!string.Equals(website, "indeed", StringComparison.OrdinalIgnoreCase))
                {
                    return NotFound();  // Returns a 404 Not Found response
                }

                var uriBuilder = new UriBuilder("https://indeed.com/jobs");
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                var search = request.Query;
                var location = request.Location;

                if (!string.IsNullOrEmpty(search))
                {
                    query["q"] = HttpUtility.UrlEncode(search);
                }
                if (!string.IsNullOrEmpty(location))
                {
                    query["l"] = HttpUtility.UrlEncode(location);
                }
                uriBuilder.Query = query.ToString();
                var url = uriBuilder.Uri.ToString();


                var response = _httpClient.GetStringAsync(url);
                var page = response.Result;

                // Load the HTML string into a parser
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(page);

                var jobDataList = new List<JobData>();

                // Get all of the different job blocks on the page
                var jobNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='job_seen_beacon']");
                if (jobNodes != null)
                {
                    // Loop through the job blocks and extract the data from the HTML
                    foreach (var jobNode in jobNodes)
                    {
                        var jobData = new JobData();

                        // Job Title
                        var jobTitleNode = jobNode.SelectSingleNode(".//h2/a/span");
                        jobData.Title = jobTitleNode.InnerText.Trim();

                        // Company Name
                        var companyNode = jobNode.SelectSingleNode(".//span[@data-testid='company-name']");
                        jobData.Company = companyNode.InnerText.Trim();

                        // Location
                        var locationNode = jobNode.SelectSingleNode(".//div[@data-testid='text-location']");
                        jobData.Location = locationNode.InnerText.Trim();

                        // Description
                        var descriptionNode = jobNode.SelectSingleNode(".//div/ul");
                        jobData.Description = descriptionNode.InnerText.Trim();

                        // Salary
                        var salaryNode = jobNode.SelectSingleNode(".//div[contains(@class, 'salary-snippet-container')]");
                        if (salaryNode != null)
                        {
                            jobData.Salary = salaryNode.InnerText.Trim();
                        }

                        jobDataList.Add(jobData);
                    }
                }

                return Ok(jobDataList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }            
        }
    }
}
