namespace Experience.API
{
    public class AppSettings
    {
        public string BaseUrl { get; set; } = string.Empty;
        public string TokenEndpoint { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
    }
}
