using WebApplication1.Application.DTOs;

namespace WebApplication1.Infrastructure.ImportExport
{
    public interface IProductImportExportService
    {
        Task<(Stream FileStream, string FileName)> ExportProductsAsync(bool uploadToBlob = false);
        Task<List<string>> ListImportFilesAsync();
    }
}
