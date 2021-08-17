namespace azure_boards_pbi_autorule.Models
{
    public class RuleConfiguration
    {
        public string Type { get; set; }

        public Rule[] Rules { get; set; }
    }
}