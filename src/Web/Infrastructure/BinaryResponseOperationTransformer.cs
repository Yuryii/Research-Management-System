using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Reflection;

namespace RMS.Web.Infrastructure;

internal sealed class BinaryResponseOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var hasBinaryAttr = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<BinaryContentAttribute>().Any();

        if (!hasBinaryAttr)
            return Task.CompletedTask;

        operation.Responses ??= [];
        operation.Responses["200"] = new OpenApiResponse
        {
            Description = "Binary file content.",
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/octet-stream"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String, Format = "binary" }
                }
            }
        };

        return Task.CompletedTask;
    }
}

[AttributeUsage(AttributeTargets.Method)]
internal sealed class BinaryContentAttribute : Attribute { }
