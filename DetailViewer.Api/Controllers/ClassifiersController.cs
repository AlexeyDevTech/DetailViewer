using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassifiersController : BaseController<Classifier>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ClassifiersController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public ClassifiersController(ApplicationDbContext context, ILogger<ClassifiersController> logger) 
            : base(context, logger)
        {
        }
    }
}