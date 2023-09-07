using _4create.Application.Enums;
using _4create.Application.Models;
using FluentValidation;

namespace _4create.Application.Requests.Validation;

public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator(IRepository repository)
    {
        RuleFor(m => m.Email).NotEmpty()
            .MaximumLength(250)
            .EmailAddress()
            .MustAsync(async (m, cancellationToken) => !await repository.EmployeeWithIdExists(m, cancellationToken))
            .WithMessage("Employee with this email already exists");

        RuleFor(m => m.Title).IsInEnum();
        
        RuleFor(m => m.CompanyIds)
            .Must(m => m.Count == m.Distinct().Count())
            .When(m => m.CompanyIds.Any())
            .WithMessage("Duplicate company ids are not allowed");

        RuleFor(m => m)
            .MustAsync(async (m, cancellationToken) => await repository.CompanyWithIdExists(cancellationToken, m.CompanyIds.ToArray()))
            .When(m => m.CompanyIds.Any())
            .OverridePropertyName(nameof(CreateEmployeeCommand.CompanyIds))
            .WithMessage("One of specified companies does not exist")
            
            .MustAsync(async (m, cancellationToken) => await repository.EmptyPositionInCompany(cancellationToken, m.Title, m.CompanyIds.ToArray()))
            .When(m => m.CompanyIds.Any())
            .OverridePropertyName(nameof(CreateEmployeeCommand.CompanyIds))
            .WithMessage("One of specified companies does not have empty position with this title");
    }
}