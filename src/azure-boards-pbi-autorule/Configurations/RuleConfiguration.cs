using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule.Configurations
{
    public class RuleConfiguration
    {
        public string Type { get; set; }

        public Rule[] Rules { get; set; }
    }
}