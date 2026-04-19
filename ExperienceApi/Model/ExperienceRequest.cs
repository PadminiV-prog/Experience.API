namespace ExperienceApi.Model;

public class ExperienceRequest
{
    public string Route { get; set; } = string.Empty;

    public object? Payload { get; set; }
}
