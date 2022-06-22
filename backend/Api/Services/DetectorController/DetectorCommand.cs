using System;

namespace Api.Services.DetectorController
{

    public class DetectorCommand
    {
        public DetectorCommandType Type { get; }
        private DetectorCommand(DetectorCommandType type)
        {
            Type = type;
        }

        public static implicit operator DetectorCommand(DetectorCommandType type) => new(type);

        public string GetExpectedResponse()
        {
            switch (Type)
            {
                case DetectorCommandType.StartStreaming:
                case DetectorCommandType.StopStreaming:
                case DetectorCommandType.TakeSnapshot:
                    return "Ok";
                case DetectorCommandType.Ping:
                    return "Pong";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string ToString()
        {
            return Type.ToString();
        }
    }

    public enum DetectorCommandType {
        StartStreaming,
        StopStreaming,
        TakeSnapshot,
        Ping,
    }
}