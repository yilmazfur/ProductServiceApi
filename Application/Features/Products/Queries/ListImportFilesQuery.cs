using MediatR;
using WebApplication1.Infrastructure.ImportExport;

namespace WebApplication1.Application.Features.Products.Queries
{
    public record ListImportFilesQuery : IRequest<List<string>>;

    public class ListImportFilesQueryHandler : IRequestHandler<ListImportFilesQuery, List<string>>
    {
        private readonly IProductImportExportService _importExportService;

        public ListImportFilesQueryHandler(IProductImportExportService importExportService)
        {
            _importExportService = importExportService;
        }

        public async Task<List<string>> Handle(ListImportFilesQuery request, CancellationToken cancellationToken)
        {
            return await _importExportService.ListImportFilesAsync();
        }
    }
}
