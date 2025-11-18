using MediatR;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Mappings;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace WebApplication1.Application.Features.Products.Queries
{
    public record GetAllProductsQuery : IRequest<IEnumerable<ProductResponse>>;

    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<ProductResponse>>
    {
        private readonly IProductRepository _repository;

        public GetAllProductsQueryHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<ProductResponse>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var products = await _repository.GetAllAsync();
            return products.ToResponse();
        }
    }
}
