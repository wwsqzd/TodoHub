

using FluentValidation;
using TodoHub.Main.Core.DTOs.Request;

namespace TodoHub.Main.Core.Validation
{
    public class UpdateUserLanguageDTOValidator : AbstractValidator<ChangeLanguageDTO>
    {
        public UpdateUserLanguageDTOValidator() 
        {
            RuleFor(x => x.Language).NotEmpty().Must(lang => lang == "en" || lang == "de");
        }
    }
}
