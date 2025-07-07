using DetailViewer.Core.Models;

namespace DetailViewer.Core.Interfaces
{
    public interface IDocumentDataServiceFactory
    {
        IDocumentDataService CreateService(DataSourceType type);
    }
}