using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ApiClient : IApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public ApiClient(ISettingsService settingsService, ILogger logger)
        {
            _settingsService = settingsService;
            _logger = logger;
            var settings = _settingsService.LoadSettings();
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(settings.ApiUrl) // Assuming ApiUrl is in settings
            };

            _jsonSerializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };
        }

        public async Task<List<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<List<T>>(content, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting data from {endpoint}", ex);
                throw;
            }
        }

        public async Task<T> GetByIdAsync<T>(string endpoint, int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{endpoint}/{id}");
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting data from {endpoint}/{id}", ex);
                throw;
            }
        }

        public async Task<T> PostAsync<T>(string endpoint, T data)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting data to {endpoint}", ex);
                throw;
            }
        }

        public async Task PutAsync<T>(string endpoint, int id, T data)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync($"{endpoint}/{id}", content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error putting data to {endpoint}/{id}", ex);
                throw;
            }
        }

        public async Task PutAsync<T>(string endpoint, T data)
        {
            try
            {
                var jsonData = JsonSerializer.Serialize(data, _jsonSerializerOptions);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error putting data to {endpoint}", ex);
                throw;
            }
        }

        public async Task DeleteAsync(string endpoint, int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{endpoint}/{id}");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting data from {endpoint}/{id}", ex);
                throw;
            }
        }

        

        public async Task<List<Assembly>> GetParentAssembliesAsync(string entity, int id)
        {
            var endpoint = ApiEndpoints.GetParentAssemblies(entity, id);
            return await GetAsync<Assembly>(endpoint);
        }

        public async Task<List<Product>> GetRelatedProductsAsync(int assemblyId)
        {
            var endpoint = ApiEndpoints.GetRelatedProducts(assemblyId);
            return await GetAsync<Product>(endpoint);
        }

        public async Task UpdateParentAssembliesAsync(string entity, int id, List<int> parentIds)
        {
            var endpoint = ApiEndpoints.GetParentAssemblies(entity, id);
            await PutAsync(endpoint, parentIds);
        }

        public async Task UpdateRelatedProductsAsync(int assemblyId, List<int> productIds)
        {
            var endpoint = ApiEndpoints.GetRelatedProducts(assemblyId);
            await PutAsync(endpoint, productIds);
        }

        public async Task<Assembly> ConvertProductToAssemblyAsync(int productId, List<int> childProductIds)
        {
            var endpoint = ApiEndpoints.ConvertProductToAssembly(productId);
            try
            {
                var jsonData = JsonSerializer.Serialize(childProductIds, _jsonSerializerOptions);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Assembly>(responseContent, _jsonSerializerOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error posting data to {endpoint}", ex);
                throw;
            }
        }
    }
}