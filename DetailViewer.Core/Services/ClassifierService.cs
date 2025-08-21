using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ClassifierService : IClassifierService
    {
        private readonly ILogger _logger;
        private readonly IApiClient _apiClient;
        private List<ClassifierData> _allClassifiers;

        public ClassifierService(ILogger logger, IApiClient apiClient)
        {
            _logger = logger;
            _apiClient = apiClient;
        }

        public async Task LoadClassifiersAsync(string filePath = null)
        {
            if (_allClassifiers != null)
            {
                return;
            }

            try
            {
                _logger.Log("Loading classifiers from API");
                var rootClassifiers = await _apiClient.GetAsync<ClassifierData>(ApiEndpoints.Classifiers);
                _allClassifiers = FlattenClassifiers(rootClassifiers);
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error loading classifiers from API: {ex.Message}", ex);
                _allClassifiers = new List<ClassifierData>();
            }
        }

        public IEnumerable<ClassifierData> GetAllClassifiers()
        {
            return _allClassifiers ?? Enumerable.Empty<ClassifierData>();
        }

        public ClassifierData GetClassifierByCode(string code)
        {
            return _allClassifiers?.FirstOrDefault(c => c.Code == code);
        }

        private List<ClassifierData> FlattenClassifiers(List<ClassifierData> classifiers)
        {
            var flattenedList = new List<ClassifierData>();
            if (classifiers == null) return flattenedList;

            foreach (var classifier in classifiers)
            {
                flattenedList.Add(classifier);
                if (classifier.Children != null && classifier.Children.Any())
                {
                    flattenedList.AddRange(FlattenClassifiers(classifier.Children));
                }
            }
            return flattenedList;
        }
    }
}
