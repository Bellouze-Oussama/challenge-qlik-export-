namespace Challenge_Qlik_Export.Services
{
    public interface IQlikExportService
    {
        Task<byte[]> ExportObjectAsImageAsync(string appId, string objectId);
        Task<byte[]> ExportObjectAsDataAsync(string appId, string objectId);
    }
}