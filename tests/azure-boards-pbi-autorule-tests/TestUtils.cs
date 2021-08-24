using System.Collections.Generic;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule_tests
{
    public static class TestUtils
    {
        private static readonly object SampleRevisionFields = new Dictionary<string, object>()
        {
            { "System.Parent", "1" },
            { "System.WorkItemType", "Task" }
        };

        private static readonly object SampleFields = new Dictionary<string, object>()
        {
            {
                "System.State", new
                {
                    newValue = "In Progress"
                }
            }
        };

        public static readonly object SampleJObject = new
        {
            eventType = "workitem.updated",
            resource = new
            {
                workItemId = 2,
                revision = new
                {
                    fields = SampleRevisionFields
                },
                fields = SampleFields
            }
        };
        
        public static readonly object SampleJObjectWithWrongEventType = new
        {
            eventType = "workitem.notUpdated",
            resource = new
            {
                workItemId = 2,
                revision = new
                {
                    fields = SampleRevisionFields
                },
                fields = SampleFields
            }
        };

        public static readonly RuleConfiguration SampleTaskRules = new RuleConfiguration
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
        
        public static readonly RuleConfiguration SampleProductBacklogItemRules = new RuleConfiguration
        {
            Type = "Product Backlog Item",
            Rules = new[]
            {
                new Rule
                {
                    IfChildState = "New",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "New",
                    AllChildren = true
                },
                new Rule
                {
                    IfChildState = "Committed",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Committed",
                    AllChildren = false
                }
            }
        };

        public static readonly RuleConfiguration SampleInvalidTaskRules = new RuleConfiguration
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