namespace azure_boards_pbi_autorule.Services
{
    public class WorkItemsService
    {
        private readonly WorkItemTrackingHttpClientService _client;

        public WorkItemsService(WorkItemTrackingHttpClientService client)
        {
            _client = client;
        }
    }
}