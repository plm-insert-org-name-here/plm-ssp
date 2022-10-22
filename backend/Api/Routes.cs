namespace Api;

public static class Routes
{
    public static class Sites
    {
        public const string List= "sites";
        public const string GetById = "sites/{id:int}";
        public const string Create = "sites";
        public const string Update = "sites/{id:int}";
        public const string Delete = "sites/{id:int}";
    }

    public static class OPUs
    {
        public const string GetById = "opus/{id:int}";
        public const string Create = "opus";
        public const string Update = "opus/{id:int}";
        public const string Delete = "opus/{id:int}";
    }

    public static class Lines
    {
        public const string GetById = "lines/{id:int}";
        public const string Create = "lines";
        public const string Update = "lines/{id:int}";
        public const string Delete = "lines/{id:int}";
    }

    public static class Stations
    {
        public const string GetById = "stations/{id:int}";
        public const string Create = "stations";
        public const string Update = "stations/{id:int}";
        public const string Delete = "stations/{id:int}";
    }

    public static class Locations
    {
        public const string GetAll = "locations";
        public const string GetAllFree = "locations/free";
        public const string GetById = "locations/{id:int}";
        public const string Create = "locations";
        public const string Update = "locations/{id:int}";
        public const string Delete = "locations/{id:int}";
        public const string StartMonitoring = "locations/{id:int}/start_monitoring";
        public const string StopMonitoring = "locations/{id:int}/stop_monitoring";
    }

    public static class Detectors
    {
        public const string GetAll = "detectors";
        public const string GetAllAttachable = "detectors/attachable";
        public const string GetById = "detectors/{id:int}";
        public const string Update = "detectors/{id:int}";
        public const string Delete = "detectors/{id:int}";
        public const string SendCommand = "detectors/{id:int}/command";
        public const string Attach = "detectors/{id:int}/attach";
        public const string Detach = "detectors/{id:int}/detach";
        public const string Controller = "/" + "detectors/controller";
        public const string DetectorHub = "/" + "detector_hub";

        public const string StreamGroupPrefix = "Stream-";
    }

    public static class Jobs
    {
        public const string GetById = "jobs/{id:int}";
        public const string Create = "jobs";
        public const string Update = "jobs/{id:int}";
        public const string Delete = "jobs/{id:int}";
        public const string List = "jobs";
    }

    public static class Tasks
    {
        public const string GetAll = "tasks";
        public const string GetById = "tasks/{id:int}";
        public const string Create = "tasks";
        public const string Update = "tasks/{id:int}";
        public const string Delete = "tasks/{id:int}";
        public const string AbandonInstance = "tasks/{id:int}/abandon";
    }

    public static class Events
    {
        public const string GetAll = "events";
        public const string GetByTemplateId = "events/{id:int}";
    }
}