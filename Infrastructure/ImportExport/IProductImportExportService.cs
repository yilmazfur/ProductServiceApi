namespace WebApplication1.Infrastructure.ImportExport
{
    public interface IProductImportExportService
    {
        Task<List<string>> ListImportFilesAsync();
    }
}
