using DetailViewer.Core.Data;
using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Text.Json.Serialization;

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

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var changeLog = new ChangeLog
            {
                EntityName = nameof(Profile),
                EntityId = profile.Id.ToString(),
                OperationType = OperationType.Create,
                Payload = JsonSerializer.Serialize(profile, options),
                Timestamp = DateTime.UtcNow
            };
            dbContext.ChangeLogs.Add(changeLog);
            await dbContext.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(Profile profile)
        {
            _logger.Log($"Updating profile: {profile.LastName}");
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Profiles.Update(profile);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var changeLog = new ChangeLog
            {
                EntityName = nameof(Profile),
                EntityId = profile.Id.ToString(),
                OperationType = OperationType.Update,
                Payload = JsonSerializer.Serialize(profile, options),
                Timestamp = DateTime.UtcNow
            };
            dbContext.ChangeLogs.Add(changeLog);

            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteProfileAsync(int profileId)
        {
            _logger.Log($"Deleting profile: {profileId}");
            using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            var profile = await dbContext.Profiles.FindAsync(profileId);
            if (profile != null)
            {
                var options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                };

                var changeLog = new ChangeLog
                {
                    EntityName = nameof(Profile),
                    EntityId = profileId.ToString(),
                    OperationType = OperationType.Delete,
                    Payload = JsonSerializer.Serialize(profile, options),
                    Timestamp = DateTime.UtcNow
                };
                dbContext.ChangeLogs.Add(changeLog);

                dbContext.Profiles.Remove(profile);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
