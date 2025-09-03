using DetailViewer.Core;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для управления записями документов (деталей).
    /// </summary>
    public class DocumentRecordService : IDocumentRecordService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DocumentRecordService"/>.
        /// </summary>
        /// <param name="apiClient">Клиент для взаимодействия с API.</param>
        /// <param name="logger">Сервис логирования.</param>
        public DocumentRecordService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<DocumentDetailRecord>> GetAllRecordsAsync()
        {
            _logger.Log("Getting all records from API");
            return await _apiClient.GetAsync<DocumentDetailRecord>(ApiEndpoints.DocumentDetailRecords);
        }

        /// <inheritdoc/>
        public async Task AddRecordAsync(DocumentDetailRecord record, ESKDNumber eskdNumber, List<int> assemblyIds)
        {
            _logger.Log($"Adding record via API: {record.Name}");
            var payload = new { Record = record, EskdNumber = eskdNumber, AssemblyIds = assemblyIds };
            await _apiClient.PostAsync(ApiEndpoints.DocumentDetailRecords, payload);
        }

        /// <inheritdoc/>
        public async Task UpdateRecordAsync(DocumentDetailRecord record, List<int> assemblyIds)
        {
            _logger.Log($"Updating record via API: {record.Name}");
            var payload = new { record, assemblyIds }; // This might need a DTO as well if it causes issues
            //await _apiClient.PutAsync(ApiEndpoints.DocumentDetailRecords, record.Id, payload);
            await _apiClient.UpdateRecord(ApiEndpoints.DocumentDetailRecords, record.Id, payload);
        }

        /// <inheritdoc/>
        public async Task DeleteRecordAsync(int recordId)
        {
            _logger.Log($"Deleting record via API: {recordId}");
            await _apiClient.DeleteAsync(ApiEndpoints.DocumentDetailRecords, recordId);
        }

        /// <inheritdoc/>
        public async Task<List<Assembly>> GetParentAssembliesForDetailAsync(int detailId)
        {
            _logger.Log($"Getting parent assemblies for detail from API: {detailId}");
            return await _apiClient.GetAsync<Assembly>($"{ApiEndpoints.DocumentDetailRecords}/{detailId}/parents/assemblies");
        }
        public async Task<List<Assembly>> GetParentProductsForDetailAsync(int detailId)
        {
            _logger.Log($"Getting parent assemblies for detail from API: {detailId}");
            return await _apiClient.GetAsync<Assembly>($"{ApiEndpoints.DocumentDetailRecords}/{detailId}/parents/products");
        }
    }
}
