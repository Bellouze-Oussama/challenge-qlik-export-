using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using System.IO;

namespace Challenge_Qlik_Export.Services
{
    public class ExcelService : IExcelService
    {
        public byte[] CreateExcelWithImageAndTable(byte[] imageData, byte[] tableData)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using (var package = new ExcelPackage())
            {
                // 1. FEUILLE IMAGE - Graphique exporté en PNG
                var imageSheet = package.Workbook.Worksheets.Add("Graphique");
                
                if (imageData != null && imageData.Length > 0 && IsValidPng(imageData))
                {
                    try
                    {
                        using (var imageStream = new MemoryStream(imageData))
                        {
                            var picture = imageSheet.Drawings.AddPicture("GraphiqueQlik", imageStream, OfficeOpenXml.Drawing.ePictureType.Png);
                            picture.SetPosition(0, 0);
                            picture.SetSize(800, 500); // Taille optimale pour les graphiques
                            
                            // Ajouter des informations
                            imageSheet.Cells["H1"].Value = "Graphique Qlik Sense";
                            imageSheet.Cells["H2"].Value = "Exporté depuis l'application Sales Analysis";
                            imageSheet.Cells["H3"].Value = $"Format: PNG ({imageData.Length} bytes)";
                        }
                    }
                    catch (Exception ex)
                    {
                        AddImageErrorContent(imageSheet, imageData, ex);
                    }
                }
                else
                {
                    AddImageErrorContent(imageSheet, imageData, null);
                }

                // 2. FEUILLE DONNÉES - Tableau exporté en XLSX
                var dataSheet = package.Workbook.Worksheets.Add("Données");
                
                if (tableData != null && tableData.Length > 0)
                {
                    try
                    {
                        // Les données sont déjà au format Excel (XLSX)
                        using (var tablePackage = new ExcelPackage(new MemoryStream(tableData)))
                        {
                            if (tablePackage.Workbook.Worksheets.Count > 0)
                            {
                                var sourceSheet = tablePackage.Workbook.Worksheets[0];
                                
                                // Copier tout le contenu
                                sourceSheet.Cells[1, 1, sourceSheet.Dimension.Rows, sourceSheet.Dimension.Columns]
                                    .Copy(dataSheet.Cells[1, 1]);
                                
                                // Ajuster les largeurs de colonnes
                                for (int i = 1; i <= sourceSheet.Dimension.Columns; i++)
                                {
                                    dataSheet.Column(i).Width = sourceSheet.Column(i).Width;
                                }
                                
                                // Ajouter un en-tête
                                dataSheet.Cells["A1"].Value = "DONNÉES BRUTES QLIK SENSE";
                                dataSheet.Cells["A1"].Style.Font.Bold = true;
                                dataSheet.Cells["A1"].Style.Font.Size = 14;
                            }
                            else
                            {
                                AddDataErrorContent(dataSheet, "Fichier XLSX vide");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        AddDataErrorContent(dataSheet, $"Erreur lecture XLSX: {ex.Message}");
                    }
                }
                else
                {
                    AddDataErrorContent(dataSheet, "Aucune donnée reçue");
                }

                return package.GetAsByteArray();
            }
        }

        private bool IsValidPng(byte[] data)
        {
            // Vérifier l'entête PNG: 89 50 4E 47 0D 0A 1A 0A
            return data.Length > 8 && 
                   data[0] == 0x89 && data[1] == 0x50 && 
                   data[2] == 0x4E && data[3] == 0x47;
        }

        private void AddImageErrorContent(ExcelWorksheet sheet, byte[] imageData, Exception ex)
        {
            sheet.Cells["A1"].Value = "GRAPHIQUE QLIK SENSE";
            sheet.Cells["A2"].Value = "Status: " + (ex != null ? "Erreur de format" : "Données manquantes");
            
            if (ex != null)
                sheet.Cells["A3"].Value = $"Erreur: {ex.Message}";
            
            sheet.Cells["A5"].Value = "Informations techniques:";
            sheet.Cells["A6"].Value = $"Taille données: {imageData?.Length ?? 0} bytes";
            sheet.Cells["A7"].Value = "Format attendu: PNG";
            
            sheet.Cells["A1:A7"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.Font.Size = 16;
        }

        private void AddDataErrorContent(ExcelWorksheet sheet, string message)
        {
            sheet.Cells["A1"].Value = "DONNÉES QLIK SENSE";
            sheet.Cells["A2"].Value = "Status: Erreur";
            sheet.Cells["A3"].Value = message;
            sheet.Cells["A5"].Value = "Format attendu: XLSX (Excel)";
            
            sheet.Cells["A1:A5"].Style.Font.Bold = true;
            sheet.Cells["A1"].Style.Font.Size = 16;
        }
    }
}