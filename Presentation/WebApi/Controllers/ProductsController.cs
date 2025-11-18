using MediatR;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Features.Products.Commands;
using WebApplication1.Application.Features.Products.Queries;

namespace WebApplication1.Presentation.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponse>>> GetAll()
        {
            _logger.LogInformation("Retrieving all products");
            var products = await _mediator.Send(new GetAllProductsQuery());
            return Ok(products);
        }

        /// <summary>
        /// Get product by code
        /// </summary>
        [HttpGet("{code}")]
        public async Task<ActionResult<ProductResponse>> GetByCode(string code)
        {
            _logger.LogInformation("Retrieving product with code: {Code}", code);
            var product = await _mediator.Send(new GetProductByCodeQuery(code));
            
            if (product == null)
            {
                _logger.LogWarning("Product with code {Code} not found", code);
                throw new KeyNotFoundException($"Product with code '{code}' not found.");
            }

            return Ok(product);
        }

        /// <summary>
        /// Create a new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ProductResponse>> Create([FromBody] CreateProductRequest request)
        {
            _logger.LogInformation("Creating product with code: {Code}", request.Code);
            var product = await _mediator.Send(new CreateProductCommand(request));
            return CreatedAtAction(nameof(GetByCode), new { code = product.Code }, product);
        }

        /// <summary>
        /// Update an existing product
        /// </summary>
        [HttpPut("{code}")]
        public async Task<ActionResult<ProductResponse>> Update(string code, [FromBody] UpdateProductRequest request)
        {
            _logger.LogInformation("Updating product with code: {Code}", code);
            var product = await _mediator.Send(new UpdateProductCommand(code, request));
            return Ok(product);
        }

        /// <summary>
        /// Delete a product
        /// </summary>
        [HttpDelete("{code}")]
        public async Task<ActionResult> Delete(string code)
        {
            _logger.LogInformation("Deleting product with code: {Code}", code);
            var result = await _mediator.Send(new DeleteProductCommand(code));
            
            if (!result)
            {
                _logger.LogWarning("Product with code {Code} not found for deletion", code);
                throw new KeyNotFoundException($"Product with code '{code}' not found.");
            }

            return NoContent();
        }
    }
}
