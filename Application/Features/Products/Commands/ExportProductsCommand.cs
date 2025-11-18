using MediatR;
using Microsoft.Extensions.Logging;
using WebApplication1.Infrastructure.ImportExport;

namespace WebApplication1.Application.Features.Products.Commands
{
    public record ExportProductsCommand(bool UploadToBlob = true) : IRequest<(Stream FileStream, string FileName)>;

    public class ExportProductsCommandHandler : IRequestHandler<ExportProductsCommand, (Stream FileStream, string FileName)>
    {
        private readonly IProductImportExportService _importExportService;
        private readonly ILogger<ExportProductsCommandHandler> _logger;

        public ExportProductsCommandHandler(
            IProductImportExportService importExportService,
            ILogger<ExportProductsCommandHandler> logger)
        {
            _importExportService = importExportService;
            _logger = logger;
        }

        public async Task<(Stream FileStream, string FileName)> Handle(ExportProductsCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Exporting all products");
            return await _importExportService.ExportProductsAsync(request.UploadToBlob);
        }
    }
}
