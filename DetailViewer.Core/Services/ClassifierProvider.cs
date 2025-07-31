
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace DetailViewer.Core.Services
{
    public class ClassifierProvider : IClassifierProvider
    {
        private readonly ILogger _logger;
        private readonly Dictionary<string, ClassifierData> _classifierData;

        public ClassifierProvider(ILogger logger)
        {
            _logger = logger;
            _logger.Log("Initializing ClassifierProvider");
            var jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eskd_classifiers.json");
            var jsonData = File.ReadAllText(jsonPath);
            var rootClassifiers = JsonSerializer.Deserialize<List<ClassifierData>>(jsonData);
            _classifierData = FlattenClassifiers(rootClassifiers).ToDictionary(c => c.Code);
        }

        public ClassifierData GetClassifierByCode(string code)
        {
            _logger.Log($"Getting classifier by code: {code}");
            _classifierData.TryGetValue(code, out var classifier);
            return classifier;
        }

        public IEnumerable<ClassifierData> GetAllClassifiers()
        {
            _logger.Log("Getting all classifiers");
            return _classifierData.Values;
        }

        private List<ClassifierData> FlattenClassifiers(List<ClassifierData> classifiers)
        {
            //_logger.Log("Flattening classifiers");
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
