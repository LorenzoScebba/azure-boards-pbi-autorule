using System;
using System.Collections.Generic;
using System.Net.Http;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;
using azure_boards_pbi_autorule.Services;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace azure_boards_pbi_autorule.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddServices(this IServiceCollection services)
        {
            services.AddSingleton<IWorkItemTrackingHttpClientService, WorkItemTrackingHttpClientService>();
            services.AddSingleton<IWorkItemsService, WorkItemsService>();
            services.AddSingleton<IRulesApplierService, RulesApplierService>();
        }
        
        public static void AddVss(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton(new HttpClient());

            var config = configuration.GetSection("Azure").Get<AzureConfiguration>();
            var rules = configuration.GetSection("Rules").Get<IEnumerable<RuleConfiguration>>();
            services.AddSingleton(rules);

            var creds = new VssBasicCredential(string.Empty, config.Pat);
            
            // Connect to Azure DevOps Services
            var connection = new VssConnection(new Uri(config.Uri), creds);
            services.AddSingleton(connection);

            var boardsClient = connection.GetClient<WorkItemTrackingHttpClient>();
            services.AddSingleton(boardsClient);
        }
    }
}