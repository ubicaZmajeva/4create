using FluentValidation;
using MediatR;
using ValidationException = FluentValidation.ValidationException;

namespace _4create.Application.Handlers.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
   
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();
        
        var context = new ValidationContext<TRequest>(request);
        var validationErrors = _validators
            .Select(x => x.ValidateAsync(context, cancellationToken))
            .SelectMany(x => x.Result.Errors)
            .Where(x => x != null)
            .DistinctBy(m => new
            {
                m.PropertyName,
                m.ErrorMessage
            })
            .ToList();

        if (!validationErrors.Any()) 
            return await next();

        throw new ValidationException(validationErrors);
    }
}