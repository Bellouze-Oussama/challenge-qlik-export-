using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace Challenge_Qlik_Export.Services
{
    public class QlikExportService : IQlikExportService
    {
        private readonly HttpClient _httpClient;
        private readonly IQlikAuthService _authService;
        private readonly QlikSettings _qlikSettings;

        public QlikExportService(HttpClient httpClient, IQlikAuthService authService, IOptions<QlikSettings> qlikSettings)
        {
            _httpClient = httpClient;
            _authService = authService;
            _qlikSettings = qlikSettings.Value;
        }

        public async Task<byte[]> ExportObjectAsImageAsync(string appId, string objectId)
        {
            try
            {
                Console.WriteLine($"📸 Export image - App: {appId}, Object: {objectId}");
                var token = await _authService.GetAuthTokenAsync();
                
                // Essayez différents endpoints
                var endpoints = new[]
                {
                    $"{_qlikSettings.TenantHostname}/api/v1/apps/{appId}/objects/{objectId}/export/image",
                    $"{_qlikSettings.TenantHostname}/qrs/app/{appId}/object/{objectId}/export/image",
                    $"{_qlikSettings.TenantHostname}/export/object/{objectId}/image",
                    $"{_qlikSettings.TenantHostname}/api/export/object/{objectId}/image"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        request.Headers.Add("Accept", "image/png");
                        request.Headers.Add("X-Qlik-User", $"UserDirectory=QLIKCLOUD;UserId={_qlikSettings.Username}");

                        var response = await _httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        
                        Console.WriteLine($"✅ Image exportée via: {endpoint}");
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Échec avec {endpoint}: {ex.Message}");
                        continue;
                    }
                }

                throw new Exception("Tous les endpoints ont échoué");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur export image: {ex.Message}");
                throw new Exception($"Erreur lors de l'export d'image: {ex.Message}", ex);
            }
        }

        public async Task<byte[]> ExportObjectAsDataAsync(string appId, string objectId)
        {
            try
            {
                Console.WriteLine($"📊 Export data - App: {appId}, Object: {objectId}");
                var token = await _authService.GetAuthTokenAsync();
                
                // Essayez différents endpoints
                var endpoints = new[]
                {
                    $"{_qlikSettings.TenantHostname}/api/v1/apps/{appId}/objects/{objectId}/export/data",
                    $"{_qlikSettings.TenantHostname}/qrs/app/{appId}/object/{objectId}/export/data",
                    $"{_qlikSettings.TenantHostname}/export/object/{objectId}/data",
                    $"{_qlikSettings.TenantHostname}/api/export/object/{objectId}/data"
                };

                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        request.Headers.Add("Accept", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        request.Headers.Add("X-Qlik-User", $"UserDirectory=QLIKCLOUD;UserId={_qlikSettings.Username}");

                        var response = await _httpClient.SendAsync(request);
                        response.EnsureSuccessStatusCode();
                        
                        Console.WriteLine($"✅ Données exportées via: {endpoint}");
                        return await response.Content.ReadAsByteArrayAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Échec avec {endpoint}: {ex.Message}");
                        continue;
                    }
                }

                throw new Exception("Tous les endpoints ont échoué");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur export données: {ex.Message}");
                throw new Exception($"Erreur lors de l'export de données: {ex.Message}", ex);
            }
        }
    }
}