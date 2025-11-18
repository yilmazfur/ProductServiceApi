using MediatR;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Mappings;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace WebApplication1.Application.Features.Products.Commands
{
    public record CreateProductCommand(CreateProductRequest Request) : IRequest<ProductResponse>;

    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ProductResponse>
    {
        private readonly IProductRepository _repository;

        public CreateProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // Check if product with same code already exists
            if (await _repository.ExistsAsync(request.Request.Code))
            {
                throw new InvalidOperationException($"Product with code '{request.Request.Code}' already exists.");
            }

            var product = request.Request.ToEntity();
            var created = await _repository.CreateAsync(product);

            return created.ToResponse();
        }
    }
}
