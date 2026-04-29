namespace Experience.API.Contract
{
    public interface ITransformAdapter
    {
        Task<string> TransformRequestAsync(string payload, string correlationId);
        Task<string> TransformResponseAsync(string payload, string correlationId);
    }
}
