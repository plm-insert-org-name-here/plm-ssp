using NSwag;
using NSwag.CodeGeneration.OperationNameGenerators;

namespace Infrastructure.OpenApi;

public class ShorterOperationNameGenerator : SingleClientFromOperationIdOperationNameGenerator
{
    private const int PathSegmentsToRemove = 2;
    
    public override string GetOperationName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
    {
        var pathSegmentsFound = 0;
        var finalIndex = 0;
        for (var i = 0; i < operation.OperationId.Length; i++)
        {
            var ch = operation.OperationId[i];

            if (char.IsUpper(ch))
                pathSegmentsFound += 1;

            if (pathSegmentsFound == PathSegmentsToRemove + 1)
            {
                finalIndex = i;
                break;
            }
        }

        operation.OperationId = operation.OperationId.Remove(0, finalIndex);
        
        return base.GetOperationName(document, path, httpMethod, operation);
    }
}
