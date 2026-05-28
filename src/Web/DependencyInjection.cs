using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RMS.Application.Application.Commands.CreateApplication;
using RMS.Application.Common.Interfaces;
using RMS.Infrastructure.Data;
using RMS.Web.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();

        builder.Services.AddScoped<IUser, CurrentUser>();

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer<ApiExceptionOperationTransformer>();
            options.AddOperationTransformer<IdentityApiOperationTransformer>();
            options.AddOperationTransformer<BinaryResponseOperationTransformer>();

            options.AddSchemaTransformer((schema, context, cancellationToken) =>
            {
                if (schema.Properties?.ContainsKey("files") == true)
                {
                    var prop = context.JsonTypeInfo.Type
                        .GetProperty("Files");

                    if (prop?.PropertyType == typeof(IFormFileCollection))
                    {
                        schema.Properties["files"] = new OpenApiSchema
                        {
                            Type = JsonSchemaType.Array,
                            Items = new OpenApiSchema
                            {
                                Type = JsonSchemaType.String,
                                Format = "binary"
                            }
                        };
                    }
                }

                return Task.CompletedTask;
            });
        });

        builder.Services.AddCors();
        builder.Services.AddAntiforgery();
    }

    public static void AddKeyVaultIfConfigured(this IHostApplicationBuilder builder)
    {
        var keyVaultUri = builder.Configuration["AZURE_KEY_VAULT_ENDPOINT"];
        if (!string.IsNullOrWhiteSpace(keyVaultUri))
        {
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri),
                new DefaultAzureCredential());
        }
    }
}
