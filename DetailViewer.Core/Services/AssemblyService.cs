using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class AssemblyService : IAssemblyService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        public AssemblyService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<Assembly>> GetAssembliesAsync()
        {
            _logger.Log("Getting all assemblies from API");
            return await _apiClient.GetAsync<Assembly>(ApiEndpoints.Assemblies);
        }

        public async Task DeleteAssemblyAsync(int assemblyId)
        {
            _logger.Log($"Deleting assembly via API: {assemblyId}");
            await _apiClient.DeleteAsync(ApiEndpoints.Assemblies, assemblyId);
        }

        public async Task AddAssemblyAsync(Assembly assembly, List<int> parentAssemblyIds, List<int> relatedProductIds)
        {
            _logger.Log($"Adding assembly via API: {assembly.Name}");
            var payload = new 
            {
                Assembly = assembly,
                EskdNumber = assembly.EskdNumber,
                ParentAssemblyIds = parentAssemblyIds,
                RelatedProductIds = relatedProductIds
            };
            await _apiClient.PostAsync(ApiEndpoints.Assemblies, payload);
        }

        public async Task UpdateAssemblyAsync(Assembly assembly)
        {
            _logger.Log($"Updating assembly via API: {assembly.Name}");
            await _apiClient.PutAsync(ApiEndpoints.Assemblies, assembly.Id, assembly);
        }

        public async Task<List<Assembly>> GetParentAssembliesAsync(int assemblyId)
        {
            _logger.Log($"Getting parent assemblies for assembly from API: {assemblyId}");
            return await _apiClient.GetAsync<Assembly>($"{ApiEndpoints.Assemblies}/{assemblyId}/parents");
        }

        public async Task UpdateAssemblyParentAssembliesAsync(int assemblyId, List<Assembly> parentAssemblies)
        {
            _logger.Log($"Updating parent assemblies for assembly via API: {assemblyId}");
            await _apiClient.PutAsync($"{ApiEndpoints.Assemblies}/{assemblyId}/parents", parentAssemblies.Select(p => p.Id).ToList());
        }

        public async Task UpdateAssemblyRelatedProductsAsync(int assemblyId, List<Product> relatedProducts)
        {
            _logger.Log($"Updating related products for assembly via API: {assemblyId}");
            await _apiClient.PutAsync($"{ApiEndpoints.Assemblies}/{assemblyId}/products", relatedProducts.Select(p => p.Id).ToList());
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int assemblyId)
        {
            _logger.Log($"Getting related products for assembly from API: {assemblyId}");
            return await _apiClient.GetRelatedProductsAsync(assemblyId);
        }

        public async Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<Product> childProducts)
        {
            _logger.Log($"Converting product to assembly via API: {productId}");
            return await _apiClient.ConvertProductToAssemblyAsync(productId, childProducts.Select(p => p.Id).ToList());
        }
    }
}