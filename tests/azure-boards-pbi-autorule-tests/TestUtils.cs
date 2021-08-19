using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule_tests
{
    public static class TestUtils
    {
        public static RuleConfiguration SampleRules = new RuleConfiguration
        {
            Type = "Task",
            Rules = new[]
            {
                new Rule
                {
                    IfChildState = "To Do",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "New",
                    AllChildren = true
                },
                new Rule
                {
                    IfChildState = "In Progress",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Committed",
                    AllChildren = false
                }
            }
        };
        
        public static RuleConfiguration SampleRulesWithWrongSetParentStateTo = new RuleConfiguration
        {
            Type = "Task",
            Rules = new[]
            {
                new Rule
                {
                    IfChildState = "To Do",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Test",
                    AllChildren = true
                },
                new Rule
                {
                    IfChildState = "In Progress",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Test",
                    AllChildren = false
                }
            }
        };
    }
}