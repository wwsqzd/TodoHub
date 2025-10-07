

using FluentValidation;
using TodoHub.Main.Core.DTOs.Request;

namespace TodoHub.Main.Core.Validation
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.Name).NotNull().MinimumLength(3);
            RuleFor(x => x.Email).NotNull().EmailAddress();
            RuleFor(x => x.Password).NotNull().MinimumLength(8);
            RuleFor(x => x.ConfirmPassword).NotNull().Equal(x => x.Password).WithMessage("Passwords do not match");
        }
    }
}
