using WebApplication1.Infrastructure.Storage;

namespace WebApplication1.Infrastructure.ImportExport
{
    public class ProductImportExportService : IProductImportExportService
    {
        private readonly IBlobStorageService _blobStorage;
        private readonly ILogger<ProductImportExportService> _logger;

        public ProductImportExportService(
            IBlobStorageService blobStorage,
            ILogger<ProductImportExportService> logger)
        {
            _blobStorage = blobStorage;
            _logger = logger;
        }

        public async Task<List<string>> ListImportFilesAsync()
        {
            return await _blobStorage.ListFilesAsync();
        }
    }
}
