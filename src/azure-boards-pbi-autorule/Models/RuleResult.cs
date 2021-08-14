namespace azure_boards_pbi_autorule.Models
{
    public class RuleResult
    {
        public bool Modified { get; set; }
        public Rule MatchedRule { get; set; }
        public string Message { get; set; }
    }
}