namespace azure_boards_pbi_autorule.Models
{
    public class AzureWebHookModel
    {
        public int workItemId { get; set; }
        public int parentId { get; set; }
        public string workItemType { get; set; }
        public string eventType { get; set; }
        public string state { get; set; }
    }
}