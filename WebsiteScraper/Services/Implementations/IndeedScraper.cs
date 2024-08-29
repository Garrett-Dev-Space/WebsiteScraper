using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Web;
using WebsiteScraper.Models;

namespace WebsiteScraper.Services.Implementations
{
    public class IndeedScraper : IWebScraper
    {
        private readonly HttpClient httpClient;
        private const string indeedBaseURL = "https://indeed.com/jobs";

        public IndeedScraper(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }
        public async Task<List<JobData>> ScrapeJobs(string search, string location, int? dayRange)
        {
            var uriBuilder = new UriBuilder(indeedBaseURL);
            var query = HttpUtility.ParseQueryString(uriBuilder.Query);
            query["sort"] = "date";

            // Add LastNDays value if valid
            if (dayRange != null && dayRange > 0)
            {
                query["fromage"] = dayRange.ToString();
            }

            // Add the search parameter
            if (!string.IsNullOrEmpty(search))
            {
                query["q"] = HttpUtility.UrlEncode(search);
            }
            else
            {
                query["q"] = "dispensary";
            }

            //Add the location parameter
            if (!string.IsNullOrEmpty(location))
            {
                query["l"] = HttpUtility.UrlEncode(location);
            }
            else
            {
                query["l"] = "california";
            }

            //Create the URL
            uriBuilder.Query = query.ToString();
            var url = uriBuilder.Uri.ToString();

            //Call the page
            var response = await httpClient.GetStringAsync(url);
            var page = response;

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
                    jobData.Title = jobNode.SelectSingleNode(".//h2/a/span")?.InnerText.Trim() ?? "N/A";

                    // Company Name
                    jobData.Company = jobNode.SelectSingleNode(".//span[@data-testid='company-name']")?.InnerText.Trim() ?? "N/A";

                    // Location
                    jobData.Location = jobNode.SelectSingleNode(".//div[@data-testid='text-location']")?.InnerText.Trim() ?? "N/A";

                    // Description
                    jobData.Description = jobNode.SelectSingleNode(".//div/ul")?.InnerText.Trim() ?? "N/A";

                    // Salary
                    jobData.Salary = jobNode.SelectSingleNode(".//div[contains(@class, 'salary-snippet-container')]")?.InnerText.Trim() ?? "N/A";

                    jobDataList.Add(jobData);
                }
            }
            return jobDataList;
        }
    }
}
