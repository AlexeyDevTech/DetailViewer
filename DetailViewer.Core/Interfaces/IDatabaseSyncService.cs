using System.Threading.Tasks;

namespace DetailViewer.Core.Interfaces
{
    public interface IDatabaseSyncService
    {
        Task SynchronizeAsync();
    }
}
