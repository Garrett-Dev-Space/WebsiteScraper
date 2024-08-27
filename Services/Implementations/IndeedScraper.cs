﻿using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Web;
using WebsiteScraper.Models;

namespace WebsiteScraper.Services.Implementations
{
    public class IndeedScraper : IWebScraper
    {
        private readonly HttpClient _httpClient;

        public IndeedScraper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<JobResultsDTO> ScrapeJobs(string search, string location, int? dayRange)
        {
            var uriBuilder = new UriBuilder("https://indeed.com/jobs");
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
            var response = await _httpClient.GetStringAsync(url);
            var page = response;

            // Load the HTML string into a parser
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(page);

            var jobDataReturn = new JobResultsDTO();
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
            jobDataReturn.Meta = new MetaData()
            {
                Query_Id = Guid.NewGuid()
            };
            jobDataReturn.Results = jobDataList;
            return jobDataReturn;
        }
    }
}
