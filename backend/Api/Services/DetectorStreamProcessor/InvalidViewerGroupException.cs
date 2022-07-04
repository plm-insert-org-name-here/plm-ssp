using System;

namespace Api.Services.DetectorStreamProcessor
{
    public class InvalidViewerGroupException : Exception
    {
        public InvalidViewerGroupException()
        {
        }

        public InvalidViewerGroupException(string? message) : base(message)
        {
        }
    }
}