namespace azure_boards_pbi_autorule.Models
{
    public class StateRule
    {
        public string IfState { get; set; }

        public string[] NotParentStates { get; set; }

        public string SetParentStateTo { get; set; }

        public string SetChildrenStateTo { get; set; }

        public bool All { get; set; }

        public string Target => string.IsNullOrWhiteSpace(SetChildrenStateTo) ? "Parent" : "Childrens";

        public string TargetRule =>
            string.IsNullOrWhiteSpace(SetChildrenStateTo) ? SetParentStateTo : SetChildrenStateTo;
    }
}