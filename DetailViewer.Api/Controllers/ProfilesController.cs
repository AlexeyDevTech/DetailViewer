using DetailViewer.Api.Data;
using DetailViewer.Api.DTOs;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : BaseController<Profile>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProfilesController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public ProfilesController(ApplicationDbContext context, ILogger<ProfilesController> logger) : base(context, logger)
        {
        }

        /// <summary>
        /// Получает все профили в виде DTO.
        /// </summary>
        /// <returns>Список всех профилей в виде DTO.</returns>
        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<ProfileDto>>> GetAllProfiles()
        {
            _logger.LogInformation("Getting all profiles as DTOs");
            return await _context.Profiles
                .Select(p => new ProfileDto
                {
                    Id = p.Id,
                    LastName = p.LastName,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    FullName = p.FullName,
                    ShortName = p.ShortName
                })
                .ToListAsync();
        }

        /// <summary>
        /// Получает профиль по ID.
        /// </summary>
        /// <param name="id">Идентификатор профиля.</param>
        /// <returns>Профиль.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<Profile>> GetProfile(int id)
        {
            _logger.LogInformation($"Getting profile with id {id}");
            var profile = await _context.Profiles.FindAsync(id);

            if (profile == null)
            {
                return NotFound();
            }

            return profile;
        }

        [HttpPost]
        public async Task<ActionResult<Profile>> PostProfile(Profile profile)
        {
            return await Post(profile);
        }
    }
}
