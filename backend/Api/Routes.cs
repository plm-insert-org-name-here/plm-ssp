namespace Api
{
    public static class Routes
    {
        private const string Prefix = "api/v1/";
        public static class Locations
        {
            public const string GetAll = Prefix + "locations";
            public const string GetById = Prefix + "locations/{id:int}";
            public const string Create = Prefix + "locations";
            public const string Update = Prefix + "locations/{id:int}";
            public const string Delete = Prefix + "locations/{id:int}";
        }

        public static class Detectors
        {
            public const string GetAll = Prefix + "detectors";
            public const string GetAllAttachable = Prefix + "detectors/attachable";
            public const string GetById = Prefix + "detectors/{id:int}";
            public const string Update = Prefix + "detectors/{id:int}";
            public const string Delete = Prefix + "detectors/{id:int}";
            public const string SendCommand = Prefix + "detectors/{id:int}/command";
            public const string Attach = Prefix + "detectors/{id:int}/attach";
            public const string Detach = Prefix + "detectors/{id:int}/detach";
            public const string Controller = "/" + Prefix + "detectors/controller";
        }

        public static class Jobs
        {
            public const string GetAll = Prefix + "jobs";
            public const string GetById = Prefix + "jobs/{id:int}";
            public const string Create = Prefix + "jobs";
            public const string Update = Prefix + "jobs/{id:int}";
            public const string Delete = Prefix + "jobs/{id:int}";
        }

        public static class Tasks
        {
            public const string GetAll = Prefix + "tasks";
            public const string GetById = Prefix + "tasks/{id:int}";
            public const string Create = Prefix + "tasks";
            public const string Update = Prefix + "tasks/{id:int}";
            public const string Delete = Prefix + "tasks/{id:int}";
        }
    }
}