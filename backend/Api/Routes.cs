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

    }
}