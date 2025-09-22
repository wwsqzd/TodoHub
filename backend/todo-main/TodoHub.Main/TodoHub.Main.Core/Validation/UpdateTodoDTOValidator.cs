
using FluentValidation;
using TodoHub.Main.Core.DTOs.Request;

namespace TodoHub.Main.Core.Validation
{
    public class UpdateTodoDTOValidator : AbstractValidator<UpdateTodoDTO>
    {
        public UpdateTodoDTOValidator()
        {
            RuleFor(x => x.Title).NotEmpty().MinimumLength(5).MaximumLength(40);
            RuleFor(x => x.Description).MaximumLength(300);
        }
    }
}
