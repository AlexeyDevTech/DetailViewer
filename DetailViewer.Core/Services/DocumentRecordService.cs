using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class DocumentRecordService : IDocumentRecordService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        public DocumentRecordService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            _logger.Log("Getting all records from API");
            return await _apiClient.GetAsync<DocumentDetailRecord>(ApiEndpoints.DocumentDetailRecords);
        }

        public async Task AddRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _logger.Log($"Adding record via API: {record.Name}");
            var payload = new { record, assemblyIds };
            await _apiClient.PostAsync(ApiEndpoints.DocumentDetailRecords, payload);
        }

        public async Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _logger.Log($"Updating record via API: {record.Name}");
            var payload = new { record, assemblyIds };
            await _apiClient.PutAsync(ApiEndpoints.DocumentDetailRecords, record.Id, payload);
        }

        public async Task DeleteRecordAsync(int recordId)
        {
            _logger.Log($"Deleting record via API: {recordId}");
            await _apiClient.DeleteAsync(ApiEndpoints.DocumentDetailRecords, recordId);
        }

        public async Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId)
        {
            _logger.Log($"Getting parent assemblies for detail from API: {detailId}");
            return await _apiClient.GetAsync<Assembly>($"{ApiEndpoints.DocumentDetailRecords}/{detailId}/parents");
        }
    }
}
