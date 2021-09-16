using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Services;
using azure_boards_pbi_autorule.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using Serilog;

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
            var stateRules = configuration.GetSection("StateRules").Get<IEnumerable<StateRuleConfiguration>>().ToList();
            var areaRules = configuration.GetSection("AreaRules").Get<IEnumerable<AreaRuleConfiguration>>().ToList();
            
            Log.Information("Starting with state rules {@rules}", stateRules);
            Log.Information("Starting with area rules {@rules}", areaRules);
            
            services.AddSingleton<IEnumerable<StateRuleConfiguration>>(stateRules);
            services.AddSingleton<IEnumerable<AreaRuleConfiguration>>(areaRules);
            
            var credentials = new VssBasicCredential(string.Empty, config.Pat);

            // Connect to Azure DevOps Services
            var connection = new VssConnection(new Uri(config.Uri), credentials);
            services.AddSingleton(connection);

            var boardsClient = connection.GetClient<WorkItemTrackingHttpClient>();
            services.AddSingleton(boardsClient);
        }
    }
}