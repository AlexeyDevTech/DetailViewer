using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
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
                // Construct path relative to the application's base directory
                string basePath = AppContext.BaseDirectory;
                string fullPath = Path.Combine(basePath, filePath);

                _logger.Log($"Loading classifiers from file: {fullPath}");

                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning($"Classifier file not found at {fullPath}");
                    _allClassifiers = new List<ClassifierData>();
                    return;
                }

                var json = await File.ReadAllTextAsync(fullPath);
                var rootClassifiers = JsonSerializer.Deserialize<List<ClassifierData>>(json);
                _allClassifiers = FlattenClassifiers(rootClassifiers);
                _logger.Log("Classifiers loaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading classifiers from file: {ex.Message}", ex);
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
