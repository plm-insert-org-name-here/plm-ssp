namespace Domain.Common;

[Flags]
public enum DetectorState
{
    Off = 0,
    Standby = 1 << 0,
    Streaming = 1 << 1,
    // NOTE(rg): Monitoring state is unused until it's decided whether the stream is broadcast by the backend
    // or a direct client->detector connection is used
    Monitoring = 1 << 2,
    Locating = 1 << 3
}