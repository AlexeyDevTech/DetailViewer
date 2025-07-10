using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DetailViewer.Core.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _dbContext;

        public ProfileService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Profile>> GetAllProfilesAsync()
        {
            return await _dbContext.Profiles.ToListAsync();
        }

        public async Task AddProfileAsync(Profile profile)
        {
            _dbContext.Profiles.Add(profile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(Profile profile)
        {
            _dbContext.Profiles.Update(profile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteProfileAsync(int profileId)
        {
            var profile = await _dbContext.Profiles.FindAsync(profileId);
            if (profile != null)
            {
                _dbContext.Profiles.Remove(profile);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
