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

    public static class TemplateStateExt
    {
        public static bool IsValid(this TemplateState state)
        {
            return state is TemplateState.Present or TemplateState.Missing;
        }
    }
}