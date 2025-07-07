using DetailViewer.Core.Interfaces;
using DetailViewer.Core.Models;
using Prism.Ioc;
using System;

namespace DetailViewer.Core.Services
{
    public class DocumentDataServiceFactory : IDocumentDataServiceFactory
    {
        private readonly IContainerProvider _containerProvider;
        private readonly AppSettings _appSettings;

        public DocumentDataServiceFactory(IContainerProvider containerProvider, AppSettings appSettings)
        {
            _containerProvider = containerProvider;
            _appSettings = appSettings;
        }

        public IDocumentDataService CreateService(DataSourceType type)
        {
            switch (type)
            {
                case DataSourceType.Excel:
                    return _containerProvider.Resolve<ExcelDocumentDataService>();
                case DataSourceType.GoogleSheets:
                    return new GoogleSheetsDocumentDataService(_appSettings.GoogleSheetsCredentialsPath, _containerProvider.Resolve<ILogger>());
                default:
                    throw new ArgumentException("Unknown data source type.");
            }
        }
    }
}