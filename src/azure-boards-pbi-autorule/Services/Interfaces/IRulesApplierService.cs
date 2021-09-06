using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule.Services.Interfaces
{
    public interface IRulesApplierService
    {
        bool HasRuleForType(string type);
        Task<Result<Rule, string>> ApplyRules(AzureWebHookModel vm);
    }
}