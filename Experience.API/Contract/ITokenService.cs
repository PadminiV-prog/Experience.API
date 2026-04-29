namespace Experience.API.Contract
{
    public interface ITokenService
    {
        Task<string> GetAccessTokenAsync();
    }
}
