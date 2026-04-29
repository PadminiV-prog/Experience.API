namespace Experience.API.Helpers
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
