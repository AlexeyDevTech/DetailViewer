using DetailViewer.Api.Data;
using DetailViewer.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DetailViewer.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductAssembliesController : BaseController<ProductAssembly>
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ProductAssembliesController"/>.
        /// </summary>
        /// <param name="context">Контекст базы данных приложения.</param>
        /// <param name="logger">Логгер для контроллера.</param>
        public ProductAssembliesController(ApplicationDbContext context, ILogger<ProductAssembliesController> logger) : base(context, logger)
        {
            _logger.LogInformation("ProductAssembliesController created");
        }
    }
}
