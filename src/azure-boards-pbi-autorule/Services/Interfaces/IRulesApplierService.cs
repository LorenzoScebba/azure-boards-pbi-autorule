using System.Threading.Tasks;
using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule.Services.Interfaces
{
    public interface IRulesApplierService
    {
        bool HasStateRuleForType(string type);
        bool HasAreaRuleForType(string type);
        Task<Result<Rule, string>> ApplyStateRules(AzureWebHookModel vm);
    }
}