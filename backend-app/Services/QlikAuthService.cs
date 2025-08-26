using System.Net.Http.Headers;
using System.Text;
using Challenge_Qlik_Export.Models;
using Microsoft.Extensions.Options;

namespace Challenge_Qlik_Export.Services
{
    public class QlikAuthService : IQlikAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly QlikSettings _qlikSettings;

        public QlikAuthService(IOptions<QlikSettings> qlikSettings, HttpClient httpClient)
        {
            _qlikSettings = qlikSettings.Value;
            _httpClient = httpClient;
        }

        public async Task<string> GetAuthTokenAsync()
        {
            try
            {
                Console.WriteLine("🔐 Tentative d'authentification avec Qlik Sense...");
                
                // Essai 1: Utiliser l'API Key directement comme token
                if (!string.IsNullOrEmpty(_qlikSettings.ApiKey))
                {
                    Console.WriteLine("✅ Utilisation de l'API Key comme token");
                    return _qlikSettings.ApiKey;
                }

                // Essai 2: Authentification avec user/mot de passe
                Console.WriteLine("🔄 Tentative avec user/mot de passe...");
                return await AuthenticateWithCredentialsAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERREUR Auth: {ex.Message}");
                throw new Exception($"Erreur d'authentification Qlik: {ex.Message}", ex);
            }
        }

        private async Task<string> AuthenticateWithCredentialsAsync()
        {
            try
            {
                var credentials = new
                {
                    username = _qlikSettings.Username,
                    password = _qlikSettings.Password
                };

                var request = new HttpRequestMessage(HttpMethod.Post, 
                    $"{_qlikSettings.TenantHostname}/api/v1/login")
                {
                    Content = new StringContent(
                        System.Text.Json.JsonSerializer.Serialize(credentials),
                        Encoding.UTF8,
                        "application/json")
                };

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                // Essayez de récupérer le token de différents headers
                if (response.Headers.TryGetValues("X-Qlik-Session", out var sessionTokens))
                    return sessionTokens.FirstOrDefault();
                
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                    return cookies.FirstOrDefault(c => c.Contains("Session"));
                
                // Si aucun token dans les headers, essayez le body
                var responseBody = await response.Content.ReadAsStringAsync();
                if (responseBody.Contains("token"))
                    return responseBody;

                throw new Exception("Token non trouvé dans la réponse");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Échec auth credentials: {ex.Message}");
                throw;
            }
        }
    }
}