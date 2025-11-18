using MediatR;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Mappings;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace WebApplication1.Application.Features.Products.Queries
{
    public record GetProductByCodeQuery(string Code) : IRequest<ProductResponse?>;

    public class GetProductByCodeQueryHandler : IRequestHandler<GetProductByCodeQuery, ProductResponse?>
    {
        private readonly IProductRepository _repository;

        public GetProductByCodeQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductResponse?> Handle(GetProductByCodeQuery request, CancellationToken cancellationToken)
        {
            var product = await _repository.GetByCodeAsync(request.Code);
            return product?.ToResponse();
        }
    }
}
