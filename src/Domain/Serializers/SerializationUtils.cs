using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Domain.Serializers;

// Props to https://stackoverflow.com/a/54774878
public static class SerializationUtils
{
    public static string GetJsonPropertyName<TClass>(Expression<Func<TClass, object>> expr)
    {
        var body = expr.Body is UnaryExpression unary ? unary.Operand : expr.Body;

        if (body is MemberExpression memberEx)
        {
            var propName = memberEx.Member.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name;
            if (propName is null)
                throw new ArgumentException("JsonPropertyName attribute is not configured for referenced field");

            return propName;
        }

        throw new ArgumentException("Expected a field access lambda");
    }

}