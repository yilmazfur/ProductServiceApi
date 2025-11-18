namespace WebApplication1.Infrastructure.Storage
{
    public interface IBlobStorageService
    {
        Task<string> UploadJsonAsync(string fileName, string jsonContent);
        Task<string?> DownloadJsonAsync(string fileName);
        Task<bool> DeleteAsync(string fileName);
        Task<List<string>> ListFilesAsync();
    }

    public class AzureBlobStorageService : IBlobStorageService
    {
        private readonly ILogger<AzureBlobStorageService> _logger;
        private readonly string? _connectionString;
        private readonly string _containerName;
        private readonly bool _isEnabled;

        public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
        {
            _logger = logger;
            _connectionString = configuration["AzureBlobStorage:ConnectionString"];
            _containerName = configuration["AzureBlobStorage:ContainerName"] ?? "product-imports";
            _isEnabled = IsValidConnectionString(_connectionString);

            if (!_isEnabled)
            {
                _logger.LogWarning("Azure Blob Storage is not configured or uses placeholder values. Blob operations will be skipped.");
            }
        }

        private static bool IsValidConnectionString(string? connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return false;
            }

            // Treat common placeholder samples as not configured
            var placeholders = new[]
            {
                "YOUR_ACCOUNT_NAME",
                "YOUR_CONNECTION_STRING",
                "your_account_name",
                "youraccountname",
                "yourname"
            };

            return !placeholders.Any(p => connectionString.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<string> UploadJsonAsync(string fileName, string jsonContent)
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("Azure Blob Storage is not configured. Skipping upload.");
                return $"local://{fileName}";
            }

            try
            {
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync(Azure.Storage.Blobs.Models.PublicAccessType.None);

                var blobClient = containerClient.GetBlobClient(fileName);
                using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonContent));
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation("File uploaded to blob storage: {FileName}", fileName);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to blob storage: {FileName}", fileName);
                throw new InvalidOperationException($"Failed to upload file to blob storage: {ex.Message}", ex);
            }
        }

        public async Task<string?> DownloadJsonAsync(string fileName)
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("Azure Blob Storage is not configured. Cannot download.");
                return null;
            }

            try
            {
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                if (!await blobClient.ExistsAsync())
                {
                    return null;
                }

                var response = await blobClient.DownloadContentAsync();
                return response.Value.Content.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file from blob storage: {FileName}", fileName);
                throw new InvalidOperationException($"Failed to download file from blob storage: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string fileName)
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("Azure Blob Storage is not configured. Cannot delete.");
                return false;
            }

            try
            {
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(fileName);

                return await blobClient.DeleteIfExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from blob storage: {FileName}", fileName);
                return false;
            }
        }

        public async Task<List<string>> ListFilesAsync()
        {
            if (!_isEnabled)
            {
                _logger.LogWarning("Azure Blob Storage is not configured. Cannot list files.");
                return new List<string>();
            }

            try
            {
                var blobServiceClient = new Azure.Storage.Blobs.BlobServiceClient(_connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                var files = new List<string>();
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    files.Add(blobItem.Name);
                }

                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing files from blob storage");
                return new List<string>();
            }
        }
    }
}
