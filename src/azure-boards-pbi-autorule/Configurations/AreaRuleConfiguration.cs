using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule.Configurations
{
    public class AreaRuleConfiguration
    {
        public string[] Type { get; set; }

        public AreaRule Rule { get; set; }
    }
}