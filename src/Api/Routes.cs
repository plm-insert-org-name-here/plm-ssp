namespace Api;

public static class Routes
{
    public static class Sites
    {
        public const string List= "sites";
        public const string GetById = "sites/{id}";
        public const string Create = "sites";
        public const string Update = "sites/{id}";
        public const string Delete = "sites/{id}";
    }

    public static class OPUs
    {
        public const string GetById = "opus/{id}";
        public const string Create = "opus";
        public const string Update = "opus/{id}";
        public const string Delete = "opus/{id}";
    }

    public static class Lines
    {
        public const string GetById = "lines/{id}";
        public const string Create = "lines";
        public const string Update = "lines/{id}";
        public const string Delete = "lines/{id}";
    }

    public static class Stations
    {
        public const string GetById = "stations/{id}";
        public const string Create = "stations";
        public const string Update = "stations/{id}";
        public const string Delete = "stations/{id}";
    }

    public static class Locations
    {
        public const string GetAllFree = "locations/free";
        public const string GetById = "locations/{id}";
        public const string Create = "locations";
        public const string Update = "locations/{id}";
        public const string Delete = "locations/{id}";
    }

    public static class Detectors
    {
        public const string List = "detectors";

        public const string Command = "detectors/{id}/command";
        public const string Snapshot = "detectors/{id}/snapshot";
        public const string Stream = "detectors/{id}/stream";

        public const string GetAllAttachable = "detectors/attachable";
        public const string GetById = "detectors/{id}";
        public const string Update = "detectors/{id}";
        public const string Delete = "detectors/{id}";

        public const string Attach = "detectors/{LocationId}/attach/{DetectorId}";
        public const string Detach = "detectors/{LocationId}/detach";

        public const string Identify = "detectors/{LocationId}/identify";
        public const string HeartBeat = "detectors/{MacAddress}/heartbeat";

    }

    public static class Jobs
    {
        public const string GetById = "jobs/{id}";
        public const string Create = "jobs";
        public const string Update = "jobs/{id}";
        public const string Delete = "jobs/{id}";
        public const string List = "jobs";
    }

    public static class Tasks
    {
        public const string GetById = "tasks/{id}";
        public const string Create = "tasks";
        public const string Update = "tasks/{id}";
        public const string Delete = "tasks/{id}";
        public const string GetObjectsAndEvents = "tasks/{TaskId}/objects_and_steps";
        public const string GetInstance = "tasks/instance/{id}";
    }

    public static class Events
    {
        public const string Create = "events";
    }
    
    public static class Users
    {
        public const string Create = "users/register";
        public const string Delete = "users/{id}";
        public const string Update = "users/{id}";
        public const string SignIn = "user";
        public const string SetRole = "users/{id}/setrole";
    }
}