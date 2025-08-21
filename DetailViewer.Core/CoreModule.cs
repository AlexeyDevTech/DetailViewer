using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Services;
using Prism.Ioc;
using Prism.Modularity;

namespace DetailViewer.Core
{
    public class CoreModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {

        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ISettingsService, JsonSettingsService>();
            containerRegistry.RegisterSingleton<IApiClient, ApiClient>();

            containerRegistry.RegisterSingleton<IClassifierService, ClassifierService>();
            containerRegistry.Register<IDocumentRecordService, DocumentRecordService>();
            containerRegistry.Register<IAssemblyService, AssemblyService>();
            containerRegistry.Register<IProductService, ProductService>();
            containerRegistry.Register<IExcelImportService, ExcelImportService>();
                        containerRegistry.Register<IExcelExportService, ExcelExportService>();
            containerRegistry.Register<IProfileService, ProfileService>();
            containerRegistry.Register<IPasswordService, PasswordService>();
            containerRegistry.Register<IActiveUserService, ActiveUserService>();
            containerRegistry.Register<IDocumentFilterService, DocumentFilterService>();
            containerRegistry.Register<ICsvExportService, CsvExportService>();
        }
    }
}
