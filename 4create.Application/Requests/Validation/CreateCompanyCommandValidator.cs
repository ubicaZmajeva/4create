using FluentValidation;

namespace _4create.Application.Requests.Validation;

public sealed class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator(IRepository repository)
    {
        RuleFor(m => m.Name)
            .NotEmpty()
            .MaximumLength(50)
            .MustAsync(async (companyName, cancellationToken) => !await repository.CompanyWithNameExists(cancellationToken, companyName))
            .WithMessage("Company with this name already exists.");
   
        RuleForEach(m => m.Employees)
            .ChildRules(employee =>
            {
                employee.RuleFor(m => new { m.Email, m.Title, m.Id })
                    .Must(m => m.Id.HasValue ^ (!string.IsNullOrWhiteSpace(m.Email) || m.Title.HasValue))
                    .WithMessage("Either Id of existing employee or Email and Title of new employee must be provide (but not both).");
                
                employee.RuleFor(m => m.Title)
                    .NotEmpty()
                    .When(m => !string.IsNullOrWhiteSpace(m.Email));
            });
        
        RuleFor(m => m)
            .MustAsync(async (company, cancellationToken) =>
                {
                    var existingTitles = await repository.FetchTitlesForEmployees(
                        cancellationToken,
                        company.Employees
                            .Where(m => m.Id.HasValue)
                            .Select(m => m.Id!.Value)
                            .ToArray());

                    var newTitles = company.Employees
                            .Where(m => m.Title.HasValue)
                            .Select(m => m.Title!.Value);

                    var titles = existingTitles.Concat(newTitles).ToList();
                    return titles.Count == titles.Distinct().Count();
                })
            .OverridePropertyName(nameof(CreateCompanyCommand.Employees))
            .WithMessage("Duplicate employee titles are not allowed.");
         
    }
}