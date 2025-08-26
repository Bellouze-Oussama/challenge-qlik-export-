namespace Challenge_Qlik_Export.Models
{
    public class QlikOAuthResponse
    {
        public string AccessToken { get; set; }
        public string TokenType { get; set; }
        public int ExpiresIn { get; set; }
    }
}