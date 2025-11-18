using MediatR;
using WebApplication1.Infrastructure.Persistence.Repositories;

namespace WebApplication1.Application.Features.Products.Commands
{
    public record DeleteProductCommand(string Code) : IRequest<bool>;

    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
    {
        private readonly IProductRepository _repository;

        public DeleteProductCommandHandler(IProductRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            return await _repository.DeleteAsync(request.Code);
        }
    }
}
