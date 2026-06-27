using FluentValidation;
using TaskManager.Web.Models.Dtos.TaskItems;

namespace TaskManager.Web.Validators.TaskItems;

public sealed class CreateTaskItemRequestValidator : AbstractValidator<CreateTaskItemRequest>
{
    public CreateTaskItemRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(5000).When(x => x.Description is not null);
        RuleFor(x => x.Status).IsInEnum();
    }
}
