namespace Challenge_Qlik_Export.Services
{
    public interface IExcelService
    {
        byte[] CreateExcelWithImageAndTable(byte[] imageData, byte[] tableData);
    }
}