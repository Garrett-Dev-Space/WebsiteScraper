using Moq;
using System.Net;
using WebsiteScraper.Services;
using WebsiteScraper.Services.Implementations;
using Xunit;

namespace WebsiteScraper.Tests.Services
{
    public class IndeedScraperTests
    {
        [Fact]
        public async Task SuccessfulExecution()
        {
            // Given
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<div class='job_seen_beacon'><h2><a><span>Sample Job</a></span></h2></div>")
            };
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);
            var scraper = new IndeedScraper(httpClient);

            // When
            var result = await scraper.ScrapeJobs("Dispensary", "California", null);

            // Then
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Sample Job", result[0].Title);
        }

        [Fact]
        public async Task NoJobsFound()
        {
            // Given
            var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("<div>Nothing in here</div>")
            };
            var mockHandler = new MockHttpMessageHandler(mockResponse);
            var httpClient = new HttpClient(mockHandler);
            var scraper = new IndeedScraper(httpClient);

            // When
            var result = await scraper.ScrapeJobs("software developer", "New York", null);

            // Then
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}