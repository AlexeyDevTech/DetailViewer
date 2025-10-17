using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Сервис для работы с децимальными номерами (ЕСКД).
    /// </summary>
    public class EskdNumberService : IEskdNumberService
    {
        private readonly IDocumentRecordService _recordService;
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="EskdNumberService"/>.
        /// </summary>
        /// <param name="recordService">Сервис для доступа к записям документов.</param>
        public EskdNumberService(IDocumentRecordService recordService, ISettingsService settingsService)
        {
            _recordService = recordService;
            _settingsService = settingsService;
        }

        /// <inheritdoc/>
        public async Task<int> GetNextDetailNumberAsync(string classCode)
        {
            var records = await _recordService.GetAllRecordsAsync();
            var allRecords = records.Where(r => r.ESKDNumber.CompanyCode == _settingsService.LoadSettings().DefaultCompanyCode).ToList();
            if (!allRecords.Any() || string.IsNullOrEmpty(classCode) || classCode.Length != 6)
            {
                return 1;
            }

            var relevantRecords = allRecords
                .Where(r => r.ESKDNumber?.ClassNumber?.Number.ToString("D6") == classCode)
                .ToList();

            if (!relevantRecords.Any())
            {
                return 1;
            }

            return relevantRecords.Max(r => r.ESKDNumber.DetailNumber) + 1;
        }

        /// <inheritdoc/>
        public async Task<int?> GetNextVersionNumberAsync(DocumentDetailRecord selectedRecord)
        {
            if (selectedRecord == null)
            {
                return null;
            }

            var allRecords = await _recordService.GetAllRecordsAsync();
            if (!allRecords.Any())
            {
                return 1;
            }

            var versions = allRecords
                .Where(r => r.ESKDNumber != null &&
                            selectedRecord.ESKDNumber != null &&
                            r.ESKDNumber.CompanyCode == selectedRecord.ESKDNumber.CompanyCode &&
                            r.ESKDNumber.ClassNumber?.Number == selectedRecord.ESKDNumber.ClassNumber?.Number &&
                            r.ESKDNumber.DetailNumber == selectedRecord.ESKDNumber.DetailNumber)
                .Select(r => r.ESKDNumber.Version)
                .OfType<int>()
                .ToList();

            return versions.Any() ? versions.Max() + 1 : 1;
        }
    }
}