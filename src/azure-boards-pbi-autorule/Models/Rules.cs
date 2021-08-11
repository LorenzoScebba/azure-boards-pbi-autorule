namespace azure_boards_pbi_autorule.Models
{
    public class RulesModel
    {
        public string Type { get; set; }

        public Rule[] Rules { get; set; }
    }

    public class Rule
    {
        public string IfChildState { get; set; }

        public string[] NotParentStates { get; set; }

        public string SetParentStateTo { get; set; }

        public bool AllChildren { get; set; }
    }
}