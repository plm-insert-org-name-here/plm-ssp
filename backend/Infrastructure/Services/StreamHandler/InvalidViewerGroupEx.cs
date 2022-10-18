using System;

namespace Application.Services.StreamHandler
{
    public class InvalidViewerGroupEx : Exception
    {
        public InvalidViewerGroupEx()
        {
        }

        public InvalidViewerGroupEx(string? message) : base(message)
        {
        }
    }
}