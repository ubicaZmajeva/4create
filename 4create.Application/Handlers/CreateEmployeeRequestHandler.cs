using _4create.Application.Models;
using _4create.Application.Requests;
using MediatR;

namespace _4create.Application.Handlers;

public class CreateEmployeeRequestHandler: IRequestHandler<CreateEmployeeCommand, int>
{
    private readonly IRepository _repository;

    public CreateEmployeeRequestHandler(IRepository repository) => _repository = repository;

    public async Task<int> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = Employee.Create(request.Email, request.Title);
        foreach (var companyId in request.CompanyIds)
        {
            employee.Companies.Add(_repository.CreateEntity<Company>(companyId));
        }
        
        return await _repository.Persist(employee) ?? throw new Exception("Failed to create employee");
    }
}