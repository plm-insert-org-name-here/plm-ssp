using System.Collections.Generic;
using NJsonSchema;

namespace Infrastructure.OpenApi;

public class ShorterTypeNameGenerator : DefaultTypeNameGenerator
{
    private const int PathSegmentsToRemove = 2;
    
    public override string Generate(JsonSchema schema, string typeNameHint, IEnumerable<string> reservedTypeNames)
    {
        var pathSegmentsFound = 0;
        var finalIndex = 0;
        for (var i = 0; i < typeNameHint.Length; i++)
        {
            var ch = typeNameHint[i];
            
            if (char.IsUpper(ch))
                pathSegmentsFound += 1;

            if (pathSegmentsFound == PathSegmentsToRemove + 1)
            {
                finalIndex = i;
                break;
            }
        }

        return base.Generate(schema, typeNameHint.Remove(0, finalIndex), reservedTypeNames);
    }
}