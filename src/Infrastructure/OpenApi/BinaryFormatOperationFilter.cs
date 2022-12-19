using System.Linq;
using NJsonSchema;
using NSwag;
using NSwag.Generation.Processors;
using NSwag.Generation.Processors.Contexts;

namespace Infrastructure.OpenApi;

public class BinaryFormatOperationFilter : IOperationProcessor
{
    public bool Process(OperationProcessorContext context)
    {
        var op = context.OperationDescription.Operation;

        if (op.Produces.Any(p => p == "application/octet-stream"))
        {
            op.Responses.Clear();
            op.Responses.Add("200", new OpenApiResponse
            {
                Schema = new JsonSchema
                {
                    Type = JsonObjectType.String,
                    Format = "binary"
                },
            });
        }

        return true;
    }
}