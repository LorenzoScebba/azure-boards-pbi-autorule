using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

namespace azure_boards_pbi_autorule.Services
{
    /// <summary>
    /// A simple wrapper around the WorkItemTrackingHttpClient for testing porpouses
    /// </summary>
    public class WorkItemTrackingHttpClientService
    {
        private readonly WorkItemTrackingHttpClient _client;

        public WorkItemTrackingHttpClientService(WorkItemTrackingHttpClient client)
        {
            _client = client;
        }
    }
}