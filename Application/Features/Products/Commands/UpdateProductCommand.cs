using MediatR;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Mappings;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace WebApplication1.Application.Features.Products.Commands
{
    public record UpdateProductCommand(string Code, UpdateProductRequest Request) : IRequest<ProductResponse>;

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ProductResponse>
    {
        private readonly IProductRepository _repository;

        public UpdateProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repository.GetByCodeAsync(request.Code);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Product with code '{request.Code}' not found.");
            }

            var product = existing.UpdateFrom(request.Request);
            var updated = await _repository.UpdateAsync(request.Code, product);

            return updated!.ToResponse();
        }
    }
}
