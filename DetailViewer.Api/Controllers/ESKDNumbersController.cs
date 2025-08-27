using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ESKDNumbersController : BaseController<ESKDNumber>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ESKDNumbersController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public ESKDNumbersController(ApplicationDbContext context, ILogger<ESKDNumbersController> logger) : base(context, logger)
        {
            _logger.LogInformation("ESKDNumbersController created");
        }
    }
}
