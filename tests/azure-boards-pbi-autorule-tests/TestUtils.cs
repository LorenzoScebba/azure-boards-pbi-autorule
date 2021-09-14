using System.Collections.Generic;
using azure_boards_pbi_autorule.Configurations;
using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule_tests
{
    public static class TestUtils
    {
        private static readonly object SampleRevisionFields = new Dictionary<string, object>
        {
            { "System.Parent", "1" },
            { "System.WorkItemType", "Task" }
        };

        private static readonly object SampleFields = new Dictionary<string, object>
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

        public static readonly StateRuleConfiguration SampleTaskStateRules = new StateRuleConfiguration
        {
            Type = "Task",
            Rules = new[]
            {
                new Rule
                {
                    IfState = "To Do",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "New",
                    All = true
                },
                new Rule
                {
                    IfState = "In Progress",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Committed",
                    All = false
                }
            }
        };
        
        public static readonly StateRuleConfiguration SampleChildrensStateRules = new StateRuleConfiguration
        {
            Type = "Product Backlog Item",
            Rules = new[]
            {
                new Rule
                {
                    IfState = "Done",
                    SetChildrenStateTo = "Done",
                },
            }
        };

        public static readonly StateRuleConfiguration SampleProductBacklogItemStateRules = new StateRuleConfiguration
        {
            Type = "Product Backlog Item",
            Rules = new[]
            {
                new Rule
                {
                    IfState = "New",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "New",
                    All = true
                },
                new Rule
                {
                    IfState = "Committed",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Committed",
                    All = false
                }
            }
        };

        public static readonly StateRuleConfiguration SampleInvalidTaskStateRules = new StateRuleConfiguration
        {
            Type = "Task",
            Rules = new[]
            {
                new Rule
                {
                    IfState = "To Do",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Test",
                    All = true
                },
                new Rule
                {
                    IfState = "In Progress",
                    NotParentStates = new[] { "Done", "Removed" },
                    SetParentStateTo = "Test",
                    All = false
                }
            }
        };
    }
}