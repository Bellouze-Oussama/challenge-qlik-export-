namespace Challenge_Qlik_Export.Services
{
    public interface IQlikAuthService
    {
        Task<string> GetAuthTokenAsync();
    }
}