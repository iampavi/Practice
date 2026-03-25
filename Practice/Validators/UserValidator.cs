using FluentValidation;
using Practice.Models;

public class UserValidator : AbstractValidator<User>
{
    public UserValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required");

        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress().WithMessage("Valid email required");

        RuleFor(x => x.Password)
            .NotEmpty().MinimumLength(3);
    }
}