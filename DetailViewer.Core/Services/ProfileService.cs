using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ILogger _logger;
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public ProfileService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ILogger logger)
        {
            _dbContextFactory = dbContextFactory;
            _logger = logger;
        }

        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            _logger.Log("Getting all profiles");
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            return await dbContext.Profiles.Select(p => new Profile
            {
                Id = p.Id,
                LastName = p.LastName,
                FirstName = p.FirstName,
                MiddleName = p.MiddleName,
                Role = p.Role,
                PasswordHash = p.PasswordHash
            }).ToListAsync();
        }

        public async Task AddProfileAsync(Profile profile)
        {
            _logger.Log($"Adding profile: {profile.LastName}");
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Profiles.Add(profile);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(Profile profile)
        {
            _logger.Log($"Updating profile: {profile.LastName}");
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Profiles.Update(profile);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteProfileAsync(int profileId)
        {
            _logger.Log($"Deleting profile: {profileId}");
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var profile = await dbContext.Profiles.FindAsync(profileId);
            if (profile != null)
            {
                dbContext.Profiles.Remove(profile);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
