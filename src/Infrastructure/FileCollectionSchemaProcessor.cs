using System;
using System.Collections.Generic;
using System.Text;
using NJsonSchema;
using NJsonSchema.Generation;

namespace RMS.Infrastructure;

public class FileCollectionSchemaProcessor : ISchemaProcessor
{
    public void Process(SchemaProcessorContext context)
    {
        var properties = context.Schema.Properties;

        if (properties.TryGetValue("files", out var fileSchema))
        {
            fileSchema.Type = JsonObjectType.Array;
            fileSchema.Item = new JsonSchema
            {
                Type = JsonObjectType.String,
                Format = "binary"
            };

            fileSchema.Reference = null;
        }
    }
}
