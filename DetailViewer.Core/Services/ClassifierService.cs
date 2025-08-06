using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ClassifierService : IClassifierService
    {
        private readonly ILogger _logger;
        private List<ClassifierData> _allClassifiers;

        public ClassifierService(ILogger logger)
        {
            _logger = logger;
        }

        public async Task LoadClassifiersAsync(string filePath = "eskd_classifiers.json")
        {
            if (_allClassifiers != null)
            {
                return;
            }

            try
            {
                if (File.Exists(filePath))
                {
                    string jsonContent = await File.ReadAllTextAsync(filePath);
                    var rootClassifiers = JsonSerializer.Deserialize<List<ClassifierData>>(jsonContent);
                    _allClassifiers = FlattenClassifiers(rootClassifiers);
                }
                else
                {
                    _logger.LogWarning($"{filePath} not found.");
                    _allClassifiers = new List<ClassifierData>();
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error loading classifiers: {ex.Message}", ex);
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
