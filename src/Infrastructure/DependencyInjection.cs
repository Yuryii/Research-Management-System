using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RMS.Application.Common.Interfaces;
using RMS.Domain.Interfaces;
using RMS.Infrastructure.Data;
using RMS.Infrastructure.Data.Interceptors;
using RMS.Infrastructure.DomainServices;
using RMS.Infrastructure.Identity;
using RMS.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
            options.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
        });

        builder.EnrichSqlServerDbContext<ApplicationDbContext>();

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddApiEndpoints();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();

        var storageProvider = builder.Configuration["Storage:Provider"];
        if (builder.Environment.IsStaging() || string.Equals(storageProvider, "S3", StringComparison.OrdinalIgnoreCase))
        {
            builder.Services.Configure<S3StorageOptions>(builder.Configuration.GetSection(S3StorageOptions.SectionName));
            builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
            builder.Services.AddAWSService<IAmazonS3>();
            builder.Services.AddTransient<IFileService, S3FileService>();
        }
        else
        {
            builder.Services.AddTransient<IFileService, LocalFileService>();
        }

        builder.Services.AddScoped<ICodeGeneratorService, CodeGeneratorService>();
        builder.Services.AddScoped<IStepResolver, StepResolver>();
    }
}
