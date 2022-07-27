namespace Api.Domain.Common
{
    public enum TemplateState
    {
        // Valid states
        Present,
        Missing,

        // "Invalid" states
        Uncertain,
        UnknownObject
    }
}