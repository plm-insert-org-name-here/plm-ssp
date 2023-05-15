using FluentResults;
using Infrastructure.Exceptions;
using Infrastructure.Logging;

namespace Infrastructure;

public static class FluentResultsExt
{
    public static void Unwrap(this Result result)
    {
        if (result.IsFailed)
        {
            // foreach (var error in result.Errors)
            // {
            //     PlmLogger.Log(error.Message);
            // }
            throw new DomainFailureException(result.Errors);
        }
    }

    public static T Unwrap<T>(this Result<T> result)
    {
        if (result.IsFailed)
        {
            // foreach (var error in result.Errors)
            // {
            //     PlmLogger.Log(error.Message);
            // }
            throw new DomainFailureException(result.Errors);
        }

        return result.Value;
    }
}