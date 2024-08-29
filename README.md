# WebsiteScraper

## Overview
WebsiteScraper is a ASP.NET Core Web API designed to scrape job listings from various websites with filters based on user input. Currently only Indeed is supported, but the project is built to be scalable so more websites could be easily added by adding more scrapers without changing the controller.

## Key Features
- **Dynamic scraping:** The application selects the appropriate scraper based on the website value entered
- **Dynamic filtering:** The application takes a set of 3 filters: query (a search value), location, and lastNDays (filters by when the job was posted). The filters are then applied to the scraper to grab the relevant data.
- **Data persistence:** The application will write all of the results to a json file where the data can later be access via a GET along with the correct query-id value. When the application starts it will grab results from the file and store them in memory to retrieve and update the results list with new query results.
- **Error handling:** All errors are handled in such a way that any issues should be easily recognizable to the user based on the returns

## Documentation
### POST /api/scrape/{website}
Scrapes the website entered for job listings
#### Request body
- query (string): The search term
- location (string): The location you want to search from
- lastNDays (int): The maximum amount of days since the job was posted
#### Response
- 200: Returns the results from the scrape
- 400: Invalid input
- 403: The website is temporarily blocking the scraping
- 404: The website entered is not supported
### GET /api/scrape/{query-id}
Gets the results of a previous scrape based on the query-id
#### Response
- 200: Returns the results from the scrape with the query-id key matching the input value
- 404: The results from query-id entered were not found

## Design Decisions
- Using dependency injection with the scraper so the application could easily be scaled up to accomodate new websites.
- Using a Guid instead of an Int for the query-id. I feel it's better this way to more easily maintain the uniqueness of the query-id key.
- All the scrape results are persisted onto a json file, which I went with because it seemed like it would be better than a csv in terms of reading and writing to it, but wouldn't require as much setup as a sqlite or post gres database. Either database options would've been better than a json file, but a json file works fine as long as the persisted results don't get too crazy (though the storage for a scraper application likely would get crazy)
- Serves the Swagger UI

## TODO
- Change out the json file storage for sqlite or postgres.
- Add authentication
- Add more scrapers
- Add more scraping filters that the user can input
- Scrape more than just one page of job results per request

