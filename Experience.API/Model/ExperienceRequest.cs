namespace Experience.API.Model
{
    public class ExperienceRequest
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
