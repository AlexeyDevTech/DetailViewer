using DetailViewer.Core;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Infrastructure.Services
{
    /// <summary>
    /// Реализация сервиса для управления профилями пользователей.
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProfileService"/>.
        /// </summary>
        /// <param name="apiClient">Клиент для взаимодействия с API.</param>
        /// <param name="logger">Сервис логирования.</param>
        public ProfileService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            _logger.Log("Getting all profiles from API");
            return await _apiClient.GetAsync<Profile>(ApiEndpoints.Profiles);
        }

        /// <inheritdoc/>
        public async Task AddProfileAsync(Profile profile)
        {
            _logger.Log($"Adding profile via API: {profile.LastName}");
            await _apiClient.PostAsync(ApiEndpoints.Profiles, profile);
        }

        /// <inheritdoc/>
        public async Task UpdateProfileAsync(Profile profile)
        {
            _logger.Log($"Updating profile via API: {profile.LastName}");
            await _apiClient.PutAsync(ApiEndpoints.Profiles, profile.Id, profile);
        }

        /// <inheritdoc/>
        public async Task DeleteProfileAsync(int profileId)
        {
            _logger.Log($"Deleting profile via API: {profileId}");
            await _apiClient.DeleteAsync(ApiEndpoints.Profiles, profileId);
        }
    }
}