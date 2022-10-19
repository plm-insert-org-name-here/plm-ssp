using System.Threading;
using System.Threading.Tasks;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api.Validation;

public static class FluentValidationExt
{
    public static async Task<ValidationResult> ValidateToModelStateAsync<T>(
        this IValidator<T> validator,
        T t,
        ModelStateDictionary modelState,
        CancellationToken ct
    )
    {
        var result = await validator.ValidateAsync(t, ct);
        if (!result.IsValid)
            foreach (var error in result.Errors)
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);

        return result;
    }
}