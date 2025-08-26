using DetailViewer.Core;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для управления классификаторами ЕСКД.
    /// </summary>
    public class ClassifierService : IClassifierService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;
        private List<Classifier>? _allClassifiers;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ClassifierService"/>.
        /// </summary>
        /// <param name="apiClient">Клиент для взаимодействия с API.</param>
        /// <param name="logger">Сервис логирования.</param>
        public ClassifierService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IEnumerable<Classifier> GetAllClassifiers()
        {
            return _allClassifiers ?? Enumerable.Empty<Classifier>();
        }

        /// <inheritdoc/>
        public Classifier? GetClassifierByNumber(int number)
        {
            return _allClassifiers?.FirstOrDefault(c => c.Number == number);
        }
    }
}
