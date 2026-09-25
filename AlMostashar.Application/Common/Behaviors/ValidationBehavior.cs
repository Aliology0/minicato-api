using AlMostashar.Domain.Shared;
using FluentValidation;
using MediatR;

namespace AlMostashar.Application.Common.Behaviors
{
    public record ValidationErrorDetail(string Property, string Error);

    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            // If there are no validators registered for this request, skip
            if (!_validators.Any())
                return await next();

            // Run all validators
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f is not null)
                .ToList();

            if (failures.Any())
            {
                var validationDetails = failures.Select(f => new ValidationErrorDetail(
                    f.PropertyName,
                    f.ErrorMessage
                )).ToList();

                var error = new Error(
                    "Validation.Error", 
                    "One or more validation errors occurred.", 
                    validationDetails);

                if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
                {
                    var resultType = typeof(TResponse);
                    var failureMethod = resultType.GetMethod("Failure", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (failureMethod != null)
                    {
                        var result = failureMethod.Invoke(null, new object[] { error });
                        return (TResponse)result!;
                    }
                }
                
                // Fallback if TResponse is not a Result<T> pattern
                throw new ValidationException(failures);
            }

            return await next();
        }
    }
}
