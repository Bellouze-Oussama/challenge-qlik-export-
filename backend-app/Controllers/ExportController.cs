using Challenge_Qlik_Export.Models;
using Challenge_Qlik_Export.Services;
using Microsoft.AspNetCore.Mvc;

namespace Challenge_Qlik_Export.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly IQlikExportService _qlikExportService;
        private readonly IExcelService _excelService;
        private readonly ILogger<ExportController> _logger;

        public ExportController(
            IQlikExportService qlikExportService, 
            IExcelService excelService,
            ILogger<ExportController> logger)
        {
            _qlikExportService = qlikExportService;
            _excelService = excelService;
            _logger = logger;
        }

        [HttpPost("export-to-excel")]
        public async Task<IActionResult> ExportToExcel([FromBody] ExportRequest request)
        {
            try
            {
                _logger.LogInformation($"Début de l'export pour l'application {request.AppId}");
                
                var imageTask = _qlikExportService.ExportObjectAsImageAsync(request.AppId, request.ImageObjectId);
                var dataTask = _qlikExportService.ExportObjectAsDataAsync(request.AppId, request.DataObjectId);

                await Task.WhenAll(imageTask, dataTask);

                var imageData = await imageTask;
                var tableData = await dataTask;

                _logger.LogInformation("Données récupérées avec succès, génération du fichier Excel...");

                var excelData = _excelService.CreateExcelWithImageAndTable(imageData, tableData);

                _logger.LogInformation("Fichier Excel généré avec succès");

                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "qlik-export.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'export");
                return StatusCode(500, $"Erreur lors de l'export: {ex.Message}");
            }
        }
    }
}