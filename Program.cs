using System.Net.Http;
using System.Text.Json;
using WebsiteScraper.Services;
using WebsiteScraper.Services.Implementations;
using WebsiteScraper.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = new LowercasePropertiesPolicy();
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

// Httpclient with headers to fake being a real user
builder.Services.AddHttpClient("ScraperClient", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36");
    client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
    client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
});

// Scrapers
builder.Services.AddSingleton<Dictionary<string, IWebScraper>>(provider =>
{
    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
    return new Dictionary<string, IWebScraper>
    {
            { "indeed", new IndeedScraper(httpClientFactory.CreateClient("ScraperClient")) },
            // More scrapers could go here
        };
});

// Register the Func<string, IWebScraper>
builder.Services.AddSingleton<Func<string, IWebScraper>>(provider =>
{
    var scrapers = provider.GetRequiredService<Dictionary<string, IWebScraper>>();
    return website => scrapers.TryGetValue(website, out var scraper) ? scraper : null;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
