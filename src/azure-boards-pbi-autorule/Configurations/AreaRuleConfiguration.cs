using System.Collections.Generic;
using azure_boards_pbi_autorule.Models;

namespace azure_boards_pbi_autorule.Configurations
{
    public class AreaRuleConfiguration
    {
        public IEnumerable<string> Type { get; set; }

        public Rule[] Rules { get; set; }
    }
}