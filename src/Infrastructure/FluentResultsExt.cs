using FluentResults;
using Infrastructure.Exceptions;

namespace Infrastructure;

public static class FluentResultsExt
{
    public static void Unwrap(this Result result)
    {
        if (result.IsFailed)
        {
            throw new DomainFailureException(result.Errors);
        }
    }

    public static T Unwrap<T>(this Result<T> result)
    {
        if (result.IsFailed)
        {
            throw new DomainFailureException(result.Errors);
        }

        return result.Value;
    }
}