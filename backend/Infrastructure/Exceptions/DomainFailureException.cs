using System;
using System.Collections.Generic;
using FluentResults;

namespace Infrastructure.Exceptions;

public class DomainFailureException : Exception
{
    public IEnumerable<IError> Errors { get; }
    public DomainFailureException(IEnumerable<IError> errors)
    {
        Errors = errors;
    }
}