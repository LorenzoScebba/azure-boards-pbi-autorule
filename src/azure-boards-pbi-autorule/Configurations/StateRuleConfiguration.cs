using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule.Configurations
{
    public class StateRuleConfiguration
    {
        public string Type { get; set; }

        public Rule[] Rules { get; set; }
    }
}