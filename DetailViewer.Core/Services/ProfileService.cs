using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IApiClient _apiClient;
        private readonly ILogger _logger;

        public ProfileService(IApiClient apiClient, ILogger logger)
        {
            _apiClient = apiClient;
            _logger = logger;
        }

        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            _logger.Log("Getting all profiles from API");
            return await _apiClient.GetAsync<Profile>(ApiEndpoints.Profiles);
        }

        public async Task AddProfileAsync(Profile profile)
        {
            _logger.Log($"Adding profile via API: {profile.LastName}");
            await _apiClient.PostAsync(ApiEndpoints.Profiles, profile);
        }

        public async Task UpdateProfileAsync(Profile profile)
        {
            _logger.Log($"Updating profile via API: {profile.LastName}");
            await _apiClient.PutAsync(ApiEndpoints.Profiles, profile.Id, profile);
        }

        public async Task DeleteProfileAsync(int profileId)
        {
            _logger.Log($"Deleting profile via API: {profileId}");
            await _apiClient.DeleteAsync(ApiEndpoints.Profiles, profileId);
        }
    }
}
