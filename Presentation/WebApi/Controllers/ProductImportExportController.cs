using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Features.Products.Commands;
using WebApplication1.Application.Features.Products.Queries;

namespace WebApplication1.Presentation.WebApi.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductImportExportController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductImportExportController> _logger;

        public ProductImportExportController(IMediator mediator, ILogger<ProductImportExportController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Upload a JSON file to blob storage for import processing
        /// The file will be automatically processed by the blob trigger function
        /// </summary>
        /// <param name="file">JSON file containing products to import</param>
        [HttpPost("import/upload")]
        public async Task<IActionResult> UploadImportFile(IFormFile file)
        {
            _logger.LogInformation("Uploading import file to blob storage");

            var result = await _mediator.Send(new UploadImportFileCommand(file));

            return Ok(new
            {
                message = "File uploaded successfully. Import will be processed automatically.",
                blobUrl = result.BlobUrl,
                fileName = result.FileName,
                fileSize = result.FileSize
            });
        }

        /// <summary>
        /// List all import files in blob storage
        /// </summary>
        [HttpGet("import/files")]
        public async Task<ActionResult<List<string>>> ListImportFiles()
        {
            _logger.LogInformation("Listing import files from blob storage");
            var files = await _mediator.Send(new ListImportFilesQuery());
            return Ok(files);
        }
    }
}
