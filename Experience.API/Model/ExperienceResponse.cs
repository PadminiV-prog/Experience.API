namespace Experience.API.Model
{
    public class ExperienceResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
