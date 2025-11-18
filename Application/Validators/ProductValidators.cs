using FluentValidation;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Features.Products.Commands;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Product code is required.")
                .MaximumLength(50).WithMessage("Product code must not exceed 50 characters.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(50).WithMessage("Product name must not exceed 50 characters.");

            RuleFor(x => x.Category)
                .NotNull().WithMessage("Category is required.")
                .Must(c => c.HasValue && Enum.IsDefined(typeof(ProductCategory), c.Value))
                .WithMessage("Category must be either Food or NonFood.");
        }
    }

    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(50).WithMessage("Product name must not exceed 50 characters.");

            RuleFor(x => x.Category)
                .NotNull().WithMessage("Category is required.")
                .Must(c => c.HasValue && Enum.IsDefined(typeof(ProductCategory), c.Value))
                .WithMessage("Category must be either Food or NonFood.");
        }
    }

    // Validators for MediatR Commands
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator(IValidator<CreateProductRequest> requestValidator)
        {
            RuleFor(x => x.Request).SetValidator(requestValidator);
        }
    }

    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator(IValidator<UpdateProductRequest> requestValidator)
        {
            RuleFor(x => x.Request).SetValidator(requestValidator);
        }
    }
}
