using _4create.Application.Models;
using _4create.Application.Requests;
using MediatR;

namespace _4create.Application.Handlers;

public class CreateCompanyRequestHandler: IRequestHandler<CreateCompanyCommand, int>
{
    private readonly IRepository _repository;

    public CreateCompanyRequestHandler(IRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<int> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company()
        {
            Name = request.Name
        };

        request.Employees.ForEach(employee => company.Employees.Add(
            employee.Id.HasValue
                ? _repository.EmployeeById(employee.Id.Value)
                : new Employee
                {
                    Email = employee.Email!,
                    Title = employee.Title!.Value
                }));
        
        return await _repository.Persist(company) ?? throw new Exception("Failed to create company");
    }
}