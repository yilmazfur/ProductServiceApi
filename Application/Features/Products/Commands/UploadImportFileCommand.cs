using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebApplication1.Infrastructure.Storage;

namespace WebApplication1.Application.Features.Products.Commands
{
    public record UploadImportFileCommand(IFormFile File) : IRequest<UploadImportFileResult>;

    public record UploadImportFileResult(string BlobUrl, string FileName, long FileSize);

    public class UploadImportFileCommandHandler : IRequestHandler<UploadImportFileCommand, UploadImportFileResult>
    {
        private readonly IBlobStorageService _blobStorageService;
        private readonly ILogger<UploadImportFileCommandHandler> _logger;

        public UploadImportFileCommandHandler(
            IBlobStorageService blobStorageService,
            ILogger<UploadImportFileCommandHandler> logger)
        {
            _blobStorageService = blobStorageService;
            _logger = logger;
        }

        public async Task<UploadImportFileResult> Handle(UploadImportFileCommand request, CancellationToken cancellationToken)
        {
            var file = request.File;

            // Validate file
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is required and cannot be empty");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName);
            if (!extension.Equals(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Only JSON files are allowed");
            }

            // Validate file size (e.g., max 10 MB)
            const long maxFileSize = 10 * 1024 * 1024; // 10 MB
            if (file.Length > maxFileSize)
            {
                throw new ArgumentException($"File size cannot exceed {maxFileSize / (1024 * 1024)} MB");
            }

            // Read file content
            string jsonContent;
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                jsonContent = await reader.ReadToEndAsync(cancellationToken);
            }

            // Validate JSON content is not empty
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                throw new ArgumentException("File content cannot be empty");
            }

            // Generate timestamped filename to ensure uniqueness
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var fileName = $"import_{timestamp}_{Path.GetFileNameWithoutExtension(file.FileName)}.json";

            _logger.LogInformation("Uploading import file to blob storage: {FileName}, Size: {Size} bytes", 
                fileName, file.Length);

            // Upload to blob storage
            var blobUrl = await _blobStorageService.UploadJsonAsync(fileName, jsonContent);

            _logger.LogInformation("File uploaded successfully to blob storage: {BlobUrl}", blobUrl);

            return new UploadImportFileResult(blobUrl, fileName, file.Length);
        }
    }
}
