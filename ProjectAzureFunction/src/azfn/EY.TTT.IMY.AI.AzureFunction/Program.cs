using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using EY.TTT.IMY.AI.Data.DBHelpers;
using EY.TTT.IMY.AI.Data.Repositories;
using EY.TTT.IMY.AI.Domain.Interfaces.Helper;
using EY.TTT.IMY.AI.Domain.Interfaces.Repositories;
using EY.TTT.IMY.AI.Domain.Interfaces.Services;
using EY.TTT.IMY.AI.Domain.Resilience;
using EY.TTT.IMY.AI.Domain.Services;
using EY.TTT.IMY.AI.Infrastructure.AzureHelper;
using EY.TTT.IMY.AI.Integration.APIHelper;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System.Data;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration((context, configBuilder) =>
    {
        var env = context.HostingEnvironment.EnvironmentName;
        configBuilder.AddJsonFile("appsettings.json", false, true)
                     .AddJsonFile($"appsettings.{env}.json", true, true)
                     .AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;
 
        var sqlConnectionString = config.GetConnectionString("SqlDb");
        services.AddScoped<IDbConnection>(_ => new SqlConnection(sqlConnectionString));

        services.AddScoped<IResiliencePolicy>(provider =>
        {
            var config = provider.GetRequiredService<IConfiguration>();
            var logger = provider.GetRequiredService<ILogger<ResiliencePolicy>>();
            return new ResiliencePolicy(config, "ResiliencePolicyConfigSection", logger);
        });

        services.AddScoped<IDBHelper, DBHelper>();

        var blobConnectionString = config["Values:AzureWebJobsStorage"];
        services.AddSingleton(new BlobServiceClient(blobConnectionString));
        var containerName = config["Values:BlobContainerName"];

        var queueConnectionString = config["Values:AzureWebJobsStorage"];
        var queueName = config["Values:QueueName"]; 
        services.AddSingleton(new QueueClient(queueConnectionString, queueName));

        services.AddSingleton<IOpenApiConfigurationOptions>(_ => new OpenApiConfigurationOptions
        {
            Info = new OpenApiInfo
            {
                Title = "IMY.AI",
                Version = "1.0"
            },
            OpenApiVersion = OpenApiVersionType.V3
        });

        services.AddScoped<ICaseSubmissionService, CaseSubmissionService>();
        services.AddScoped<ICaseFileExtractionService, CaseFileExtractionService>();
        services.AddScoped<ICaseFileStorageRepository, CaseFileStorageRepository>();
        services.AddScoped<IStorageAccountHandler, StorageAccountHandler>();
        services.AddScoped<IAPIHelper, APIHelper>();
        services.AddHttpClient();

        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddConsole();
            loggingBuilder.AddApplicationInsights(); 
        });

        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .ConfigureLogging(logging =>
    {
        logging.SetMinimumLevel(LogLevel.Information); 
    })
    .Build();

host.Run();
