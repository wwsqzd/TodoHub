

using FluentValidation;
using TodoHub.Main.Core.DTOs.Request;

namespace TodoHub.Main.Core.Validation
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        }
    }
}
