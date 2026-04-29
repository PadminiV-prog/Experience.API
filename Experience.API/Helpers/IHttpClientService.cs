namespace Experience.API.Helpers
{
    public interface IHttpClientService
    {
        Task<string> GetAsync(string url, string accessToken, string correlationId);
        Task<string> PostAsync(string url, string requestBody, string accessToken, string correlationId);
        Task<string> PutAsync(string url, string requestBody, string accessToken, string correlationId);
        Task<string> DeleteAsync(string url, string accessToken, string correlationId);
    }
}
