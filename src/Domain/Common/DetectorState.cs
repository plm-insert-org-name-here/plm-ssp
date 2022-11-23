namespace Domain.Common;

[Flags]
public enum DetectorState
{
    Off,
    Standby,
    Streaming,
    // NOTE(rg): Monitoring state is unused until it's decided whether the stream is broadcast by the backend
    // or a direct client->detector connection is used
    Monitoring,
    Locating
}