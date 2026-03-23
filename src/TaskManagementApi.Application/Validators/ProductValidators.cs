using FluentValidation;
using TaskManagementApi.Application.DTOs.Products;

namespace TaskManagementApi.Application.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.VersionName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PlannedReleaseDate).NotEmpty();
        }
    }

    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.VersionName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.PlannedReleaseDate).NotEmpty();
        }
    }
}
