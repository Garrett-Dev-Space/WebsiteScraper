namespace WebsiteScraper.Utilities
{
    using System.Text.Json;

    public class LowercasePropertiesPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.ToLower();
        }
    }
}
