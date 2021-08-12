namespace azure_boards_pbi_autorule.Models
{
    public class AzureWebHookModel
    {
        public string organization { get; set; }
        public string pat { get; set; }
        public int workItemId { get; set; }
        public string workItemType { get; set; }
        public int parentId { get; set; }
        public int parentUrl { get; set; }
        public string eventType { get; set; }
        public int rev { get; set; }
        public string teamProject { get; set; }
        public string url { get; set; }
        public string state { get; set; }
    }
}