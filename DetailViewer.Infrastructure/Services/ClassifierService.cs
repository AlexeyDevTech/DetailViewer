using DetailViewer.Core;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace DetailViewer.Infrastructure.Services
{
    ///// <summary>
    ///// Реализация сервиса для управления классификаторами ЕСКД.
    ///// </summary>
    //public class ClassifierService : IClassifierService
    //{
    //    private readonly IApiClient _apiClient;
    //    private readonly ILogger _logger;
    //    private List<Classifier>? _allClassifiers;

    //    /// <summary>
    //    /// Инициализирует новый экземпляр класса <see cref="ClassifierService"/>.
    //    /// </summary>
    //    /// <param name="apiClient">Клиент для взаимодействия с API.</param>
    //    /// <param name="logger">Сервис логирования.</param>
    //    public ClassifierService(IApiClient apiClient, ILogger logger)
    //    {
    //        _apiClient = apiClient;
    //        _logger = logger;
    //    }

    //    /// <inheritdoc/>
    //    public async Task LoadClassifiersAsync()
    //    {
    //        if (_allClassifiers != null)
    //        {
    //            return;
    //        }

    //        try
    //        {
    //            _logger.Log("Loading classifiers from API");
    //            _allClassifiers = await _apiClient.GetAsync<Classifier>(ApiEndpoints.Classifiers);
    //            _logger.Log("Classifiers loaded successfully from API.");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError($"Error loading classifiers from API: {ex.Message}", ex);
    //            _allClassifiers = new List<Classifier>();
    //        }
    //    }

    //    /// <inheritdoc/>
    //    public IEnumerable<Classifier> GetAllClassifiers()
    //    {
    //        return _allClassifiers ?? Enumerable.Empty<Classifier>();
    //    }

    //    /// <inheritdoc/>
    //    public Classifier? GetClassifierByNumber(int number)
    //    {
    //        return _allClassifiers?.FirstOrDefault(c => c.Number == number);
    //    }
    //}

    public class ClassifierService : IClassifierService
    {
        private readonly ILogger _logger;
        private List<ClassifierData>? _allClassifiers;

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

        public ClassifierData? GetClassifierByCode(string code)
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

    public static class ClassifierExtension
    {
        public static Classifier ConvertToClassifier(this ClassifierData Data)
        {
            return new Classifier
            {
                Number = Convert.ToInt32(Data.Code),
                Name = Data.Description,
            };
        }
    }
}
