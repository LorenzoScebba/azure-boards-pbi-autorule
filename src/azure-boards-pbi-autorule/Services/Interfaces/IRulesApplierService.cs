using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace azure_boards_pbi_autorule.Services.Interfaces
{
    public interface IRulesApplierService
    {
        Task<Result<Rule, string>> ApplyRules(AzureWebHookModel vm, WorkItem parentWorkItem);
    }
}