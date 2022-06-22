namespace Api
{
    public static class Routes
    {
        public static class Locations
        {
            public const string GetAll = "locations";
            public const string GetById = "locations/{id:int}";
            public const string Create = "locations";
            public const string Update = "locations/{id:int}";
            public const string Delete = "locations/{id:int}";
        }

        public static class Detectors
        {
            public const string GetAll = "detectors";
            public const string GetById = "detectors/{id:int}";
            public const string Update = "detectors/{id:int}";
            public const string Delete = "detectors/{id:int}";
            public const string SendCommand = "detectors/{id:int}/command";
            public const string Controller = "/detectors/controller";
        }
    }
}