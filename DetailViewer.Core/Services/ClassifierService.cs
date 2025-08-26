using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ClassifierService : IClassifierService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private List<Classifier>? _allClassifiers;

        public ClassifierService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task LoadClassifiersAsync()
        {
            if (_allClassifiers != null)
            {
                return;
            }

            try
            {
                _logger.Log("Loading classifiers from API");
                _allClassifiers = await _apiClient.GetAsync<Classifier>(ApiEndpoints.Classifiers);
                _logger.Log("Classifiers loaded successfully from API.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading classifiers from API: {ex.Message}", ex);
                _allClassifiers = new List<Classifier>();
            }
        }

        public IEnumerable<Classifier> GetAllClassifiers()
        {
            return _allClassifiers ?? Enumerable.Empty<Classifier>();
        }

        public Classifier? GetClassifierByNumber(int number)
        {
            return _allClassifiers?.FirstOrDefault(c => c.Number == number);
        }
    }
}